# 研究報告：Carrier ID Reader Implementation

**Branch**: `002-carrier-id-reader-impl` | **Date**: 2026-03-10

## 研究任務摘要

| 編號 | 研究任務 | 狀態 |
|------|---------|------|
| R-01 | 繼承架構設計（基底類別 vs 四個子類別） | ✅ 已解決 |
| R-02 | Barcode BL600 協定流程分析 | ✅ 已解決 |
| R-03 | Omron RFID 協定流程分析（ASCII / HEX） | ✅ 已解決 |
| R-04 | Hermes RFID 協定流程分析 | ✅ 已解決 |
| R-05 | 信號同步機制（C++ semaphore → C# 等效） | ✅ 已解決 |
| R-06 | 錯誤碼定義（-300 ~ -399 範圍） | ✅ 已解決 |
| R-07 | IConnector 事件驅動通訊模式 | ✅ 已解決 |
| R-08 | 頁面位元遮罩計算邏輯（Omron ReadRFID/WriteRFID） | ✅ 已解決 |
| R-09 | Hermes 校驗碼計算邏輯（XOR + 加法） | ✅ 已解決 |

---

## R-01: 繼承架構設計

### 決策
使用基底類別 + 繼承架構：`CarrierIDReader`（基底）→ 4 個子類別。

### 理由
- PlantUML 類別圖明確定義此架構
- 基底類別提取共用邏輯：建構函式注入、IConnector 管理、日誌、並行保護、組態
- 子類別僅覆寫設備專屬的協定流程
- 與 LoadportActor 模式一致（建構函式注入、事件訂閱屬性模式）

### 替代方案
- **Strategy Pattern（組合）**：考慮過，但 PlantUML 設計圖已指定繼承，且四種讀取器的建構函式參數一致，繼承更簡潔。不採用。
- **介面獨立實作（無基底類別）**：過多重複程式碼，違反 DRY 原則。不採用。

---

## R-02: Barcode BL600 協定流程

### 決策
從 `lp204.cc` CBL600 類別提取完整的讀取流程，轉換為 C# 事件驅動模式。

### 協定分析

**命令格式**：ASCII 字串 + `0x0D` 終結符

| 命令 | 發送內容 | 預期回應 | 逾時 |
|------|---------|---------|------|
| MOTORON | `"MOTORON" + 0x0D` | `"OK" + 0x0D` | 10 秒 |
| MOTOROFF | `"MOTOROFF" + 0x0D` | `"OK" + 0x0D` | 3 秒 |
| LON (觸發讀取) | `"LON" + 0x0D` | 條碼資料 + `0x0D` | 5 秒 |
| LOFF (停止讀取) | `"LOFF" + 0x0D` | 無（清理命令） | - |

**讀取流程**：
1. 連線 (`IConnector.Connect()`)
2. MotorON → 等待 ACK "OK"
3. 迴圈（最多 3 次重試，原 C++ 為 7 次但 spec clarification 定為 3）：
   - 發送 LON → 等待條碼資料
   - 若收到 "NG" → 繼續重試
   - 若收到有效條碼（長度 > 2）→ 跳出迴圈
   - 若逾時 → 發送 LOFF，回報錯誤
4. MotorOFF
5. 斷開連線 (`IConnector.Disconnect()`)

**回應解析**：
- 回應以 `0x0D` 結尾
- "OK" 回應 → ACK 信號
- 資料回應（長度 > 2 且非 "NG"）→ 有效條碼
- "NG" 回應 → 無法讀取，需重試

### 替代方案
- 原 C++ 使用 POSIX semaphore (`sem_timedwait`)。C# 使用 `ManualResetEventSlim` + `Wait(timeout)` 替代。

---

## R-03: Omron RFID 協定流程（ASCII / HEX）

### 決策
ASCII 與 HEX 模式共用相同的命令框架，僅在內容格式位元（frame[2]）不同。分為兩個子類別以符合使用者要求。

### 協定分析

**命令框架**（13 bytes 讀取 / 29 bytes 寫入）：

```
[0-1]  命令類型     "01" = Read, "02" = Write
[2]    內容格式     '1' = ASCII, '0' = HEX
[3]    傳輸操作     '0' = Single Trigger
[4-11] 頁面規格     位元遮罩（見 R-08）
[12]   終結符       0x0D
[12-27] 寫入資料    （僅 Write 命令，16 bytes）
[28]   終結符       0x0D（僅 Write 命令）
```

**回應格式**：
- `[0-1]` 完成碼：`"00"` = 成功，其他 = 錯誤
- `[2..n-1]` 資料參數（成功時）
- `[n]` 終結符 `0x0D`

**讀取流程**：
1. 檢查 idle 狀態
2. 根據頁碼與格式計算頁面規格位元遮罩
3. 組裝命令框架
4. 發送命令，等待回應（逾時 3 秒，但 spec 定為 10 秒）
5. 檢查完成碼 → 成功則解析資料，失敗則回報錯誤

**寫入流程**：同讀取，但命令類型為 "02"，附加 16 bytes 寫入資料

### ASCII vs HEX 差異
- ASCII 模式：每頁 16 個 ASCII 字元，讀取時自動讀取連續兩頁
- HEX 模式：每頁 8 bytes（16 hex 字元），讀取時只讀單頁
- 頁面位元遮罩計算邏輯相同，但選擇的位元值不同

### 替代方案
- 合併 ASCII/HEX 為一個類別（建構函式參數區分）：可行但使用者明確要求兩個獨立檔案。

---

## R-04: Hermes RFID 協定流程

### 決策
從 `lp204.cc` CHermos 類別提取完整流程，保留校驗碼計算邏輯。

### 協定分析

**命令框架**（`prepCmd` 函式）：

```
[0]        起始符    'S'
[1-2]      訊息長度  hex 高低位元
[3..3+n-1] 訊息內容  (命令 + 參數)
[3+n]      結束符    0x0D
[4+n]      XOR 校驗高位
[5+n]      XOR 校驗低位
[6+n]      加法校驗高位
[7+n]      加法校驗低位
```

**回應命令字元**：
- `'x'` = 讀取成功回應（含資料）
- `'w'` = 寫入成功回應
- `'v'` = 版本查詢回應
- `'e'` = 錯誤回應（含失敗碼）
- `'n'` = 硬體重置事件（非請求回應）

**讀取命令**（單頁）：
- 訊息：`"X0" + 頁碼兩位數`（如 `"X001"` = 第 1 頁）
- 頁碼範圍：1-17

**寫入命令**：
- 訊息：`"W0" + 頁碼兩位數 + 16 字元資料`
- 頁碼範圍：1-17
- 資料長度：固定 16 字元

**多頁讀取**（ReadMULTIPAGE）：
- 頁碼：98 或 99（特殊多頁讀取碼）
- 迴圈接收多個回應直到 infolen == 0

**狀態機**：IDLE → WAITACK（發送命令後）→ IDLE（收到回應或逾時）

### 回應完整性驗證（FR-010）
- XOR 校驗碼：所有位元組（起始符到結束符）XOR 運算
- 加法校驗碼：所有位元組（起始符到結束符）加總取低 8 位
- 兩個校驗碼皆以 hex 字元表示（高低位分開）

### 替代方案
- 無。Hermes 協定為專有協定，必須完全匹配參考實作。

---

## R-05: 信號同步機制

### 決策
使用 `ManualResetEventSlim` 替代 POSIX `sem_t`。

### 理由
- `ManualResetEventSlim` 是 .NET Framework 4.7.2 中最輕量的信號機制
- 支援 `Wait(int millisecondsTimeout)` 方法，等效於 `sem_timedwait`
- `Reset()` 等效於 `sem_reset`（清空信號）
- `Set()` 等效於 `sem_post`（發出信號）

### 模式
```
基底類別：
  _ackSignal = new ManualResetEventSlim(false)

發送命令前：
  _ackSignal.Reset()

等待回應：
  bool received = _ackSignal.Wait(timeoutMs)

DataReceived 事件處理：
  解析回應 → _ackSignal.Set()
```

### 替代方案
- `SemaphoreSlim`：功能更接近 POSIX sem_t，但 `ManualResetEventSlim` 語意更清晰。
- `AutoResetEvent`：不需 Reset 呼叫，但 `ManualResetEventSlim` 效能更佳且與 LoadportActor 模式一致。

---

## R-06: 錯誤碼定義

### 決策
在 -300 ~ -399 範圍內定義 CarrierID Reader 模組專屬錯誤碼。

### 錯誤碼對應表

| ErrorCode 名稱 | 值 | 說明 | 對應 C++ 錯誤 |
|----------------|-----|------|---------------|
| CarrierIdError | -300 | 基底錯誤（通用） | — |
| CarrierIdSemaphoreResetFailed | -301 | 信號重置失敗 | res=1 (sem_reset) |
| CarrierIdTimeout | -302 | 設備回應逾時 | res=2/5 (timedwait) |
| CarrierIdCommandFailed | -303 | 設備回報命令失敗 | res=6/7 (ACKED_ERR / RESERR) |
| CarrierIdBusy | -304 | 前一操作仍在進行中 | res=2 (busy) |
| CarrierIdInvalidPage | -305 | 無效頁碼 | res=1 (illegal page) |
| CarrierIdInvalidParameter | -306 | 無效參數（如寫入資料長度不對） | res=3 (invalid param) |
| CarrierIdResponseTooLong | -307 | 回應資料超出緩衝區 | res=7 (msg too long) |
| CarrierIdChecksumError | -308 | 校驗碼驗證失敗（Hermes） | checksum mismatch |
| CarrierIdConnectFailed | -309 | 連線失敗 | OpenPort fail |
| CarrierIdMotorOnFailed | -310 | MotorON 失敗（BarcodeReader） | MotorON res≠0 |
| CarrierIdMotorOffFailed | -311 | MotorOFF 失敗（BarcodeReader） | MotorOFF res≠0 |
| CarrierIdReadFailed | -312 | 讀取失敗（重試耗盡或 NG） | DoTest loop exhaust |
| CarrierIdInternalError | -313 | 內部錯誤（框架組裝失敗等） | res=3 (prepCmd) |

### 替代方案
- 使用 `const int` 欄位：可行，但 `ErrorCode` 列舉已建立。在 ErrorCode.cs 中新增列舉值。

---

## R-07: IConnector 事件驅動通訊模式

### 決策
沿用 LoadportActor 的 `IConnector` 使用模式。

### 模式分析
- `IConnector.Send(byte[], int)` → 發送命令
- `IConnector.DataReceived` 事件 → 接收回應
- `IConnector.Connect()` / `Disconnect()` → 連線管理
- 屬性 setter 管理事件訂閱/取消訂閱（憲章要求）

### 重要注意
- `Send()` 接受 `byte[]`，需將 ASCII 命令字串轉為 byte 陣列
- `DataReceived` 事件提供 `byte[]` 和 `int length`
- 需在 `OnDataReceived` 中解析回應並發出信號

---

## R-08: 頁面位元遮罩計算（Omron）

### 決策
直接移植 C++ 的 switch-case 頁面位元遮罩邏輯。

### 邏輯說明
- 命令框架 frame[4-11] 共 8 bytes 代表 30 個頁面的位元遮罩
- 每 4 個頁面佔一個 nibble（半位元組）
- ASCII 模式讀取連續兩頁（位元遮罩較大），HEX 模式讀取單頁
- 頁碼對應 offset 公式：
  - page 1-2：固定位置 frame[10-11]
  - page 3+：`offset = page / 4` 或 `(page-n) / 4` 依 page%4 餘數決定

### 替代方案
- 用公式化計算替代 switch-case：複雜度更高且可讀性差。保留 switch-case 與原 C++ 一致。

---

## R-09: Hermes 校驗碼計算

### 決策
在 C# 中精確重現 `prepCmd()` 的雙校驗碼計算。

### 演算法

**XOR 校驗碼**：
1. 對 frame[0] 到 frame[end_sign] 的所有位元組做 XOR
2. 結果取高低 nibble，各別轉為 hex 字元 ('0'-'9', 'A'-'F')

**加法校驗碼**：
1. 對 frame[0] 到 frame[end_sign] 的所有位元組做加總
2. 取結果的低 8 位
3. 高低 nibble 各別轉為 hex 字元

**完整框架**：`'S' + len_high + len_low + message + 0x0D + xor_high + xor_low + add_high + add_low`

### 回應驗證
接收回應時，重新計算 XOR 與加法校驗碼，與回應中的校驗碼比對。不一致則拒絕回應（FR-010）。
