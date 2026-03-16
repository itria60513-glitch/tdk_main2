# 資料模型：Carrier ID Reader Implementation

**Branch**: `002-carrier-id-reader-impl` | **Date**: 2026-03-10

## 實體關係圖

```
ICarrierIDReader (介面)
    ^
    |  implements
CarrierIDReader (基底類別, abstract)
    ^
    |  inherits
    ├── IDReaderBarcodeReader
    ├── IDReaderOmronASCII
    ├── IDReaderOmronHex
    └── IDReaderHermesRFID

CarrierIDReaderConfig ──> CarrierIDReader (建構函式注入)
IConnector ──> CarrierIDReader (建構函式注入)
ILogUtility ──> CarrierIDReader (建構函式注入)
```

---

## 列舉

### CarrierIDReaderType

| 值 | 名稱 | 說明 |
|----|------|------|
| 0 | BarcodeReader | Barcode BL600 條碼讀取器 |
| 1 | OmronASCII | Omron ASCII RFID 讀取器 |
| 2 | OmronHex | Omron HEX RFID 讀取器 |
| 3 | HermesRFID | Hermes RFID 讀取器 |

### ErrorCode（新增項目，-300 ~ -399 範圍）

| 值 | 名稱 | 說明 |
|----|------|------|
| -300 | CarrierIdError | CarrierID Reader 基底錯誤 |
| -301 | CarrierIdSemaphoreResetFailed | 信號重置失敗 |
| -302 | CarrierIdTimeout | 設備回應逾時 |
| -303 | CarrierIdCommandFailed | 設備回報命令失敗 |
| -304 | CarrierIdBusy | 前一操作仍在進行中 |
| -305 | CarrierIdInvalidPage | 無效頁碼 |
| -306 | CarrierIdInvalidParameter | 無效參數 |
| -307 | CarrierIdResponseTooLong | 回應資料超出緩衝區 |
| -308 | CarrierIdChecksumError | 校驗碼驗證失敗 |
| -309 | CarrierIdConnectFailed | 連線失敗 |
| -310 | CarrierIdMotorOnFailed | MotorON 失敗 |
| -311 | CarrierIdMotorOffFailed | MotorOFF 失敗 |
| -312 | CarrierIdReadFailed | 讀取失敗（重試耗盡） |
| -313 | CarrierIdInternalError | 內部錯誤 |

---

## 介面

### ICarrierIDReader

| 成員 | 型別 | 說明 |
|------|------|------|
| CarrierIDReaderConfigPath | `string` (property) | 組態檔案路徑 |
| CarrierIDReaderType | `CarrierIDReaderType` (property) | 讀取器型別 |
| ParseCarrierIDReaderData(command) | `ErrorCode` (method) | 解析裝置回應原始資料 |
| GetCarrierID(out carrierID) | `ErrorCode` (method) | 讀取載體 ID |
| SetCarrierID(carrierID) | `ErrorCode` (method) | 寫入載體 ID（僅 RFID 類型支援） |

---

## 類別

### CarrierIDReaderConfig

| 欄位 | 型別 | 必填 | 說明 | 驗證規則 |
|------|------|------|------|---------|
| ReaderType | `CarrierIDReaderType` | 是 | 讀取器型別 | 有效列舉值 |
| TimeoutMs | `int` | 是 | 預設逾時（毫秒） | 預設 10000 |
| MaxRetryCount | `int` | 是 | 最大重試次數（Barcode 專用） | 預設 3 |
| Page | `int` | 否 | RFID 頁碼（Omron / Hermes 專用） | 1-30（Omron）, 1-17（Hermes） |

### CarrierIDReader（基底類別，abstract）

| 成員 | 存取範圍 | 型別 | 說明 |
|------|---------|------|------|
| _logger | private readonly | `ILogUtility` | 日誌工具 |
| Config | public | `CarrierIDReaderConfig` | 組態（property） |
| Connector | internal | `IConnector` | 通訊介面（property with 事件訂閱管理） |
| _ackSignal | protected | `ManualResetEventSlim` | ACK/回應信號 |
| _isBusy | private volatile | `bool` | 並行操作保護旗標 |
| _receivedData | protected | `byte[]` | 最後接收的原始資料 |
| _receivedLength | protected | `int` | 最後接收的資料長度 |
| CarrierIDReaderConfigPath | public | `string` | 組態路徑 (ICarrierIDReader) |
| CarrierIDReaderType | public abstract | `CarrierIDReaderType` | 讀取器型別 (ICarrierIDReader) |
| OnDataReceived(byte[], int) | protected virtual | `void` | DataReceived 事件處理（子類別可覆寫） |
| ParseCarrierIDReaderData(string) | public abstract | `ErrorCode` | 解析回應（子類別實作） |
| GetCarrierID(out string) | public abstract | `ErrorCode` | 讀取 ID（子類別實作） |
| SetCarrierID(string) | public virtual | `ErrorCode` | 寫入 ID（預設回傳不支援，RFID 子類別覆寫） |
| SendCommand(byte[]) | protected | `ErrorCode` | 封裝 IConnector.Send |
| WaitForResponse(int) | protected | `bool` | 封裝信號等待 + 逾時 |
| CheckBusy() | protected | `ErrorCode` | 檢查並設定 busy 旗標 |
| ReleaseBusy() | protected | `void` | 釋放 busy 旗標 |

### IDReaderBarcodeReader

| 成員 | 存取範圍 | 型別 | 說明 |
|------|---------|------|------|
| CarrierIDReaderType | public override | `CarrierIDReaderType` | 回傳 `BarcodeReader` |
| MotorON() | public | `ErrorCode` | 開啟馬達 |
| ReadBarCode(out string) | public | `ErrorCode` | 讀取條碼 |
| MotorOFF() | public | `ErrorCode` | 關閉馬達 |
| GetCarrierID(out string) | public override | `ErrorCode` | 完整讀取流程（Connect → MotorON → Loop(ReadBarCode) → MotorOFF → Disconnect） |
| ParseCarrierIDReaderData(string) | public override | `ErrorCode` | 解析 BL600 回應 |
| OnDataReceived(byte[], int) | protected override | `void` | 處理 BL600 回應（ACK / 條碼資料） |

### IDReaderOmronASCII

| 成員 | 存取範圍 | 型別 | 說明 |
|------|---------|------|------|
| CarrierIDReaderType | public override | `CarrierIDReaderType` | 回傳 `OmronASCII` |
| GetCarrierID(out string) | public override | `ErrorCode` | ASCII 模式讀取流程 |
| SetCarrierID(string) | public override | `ErrorCode` | ASCII 模式寫入流程 |
| ParseCarrierIDReaderData(string) | public override | `ErrorCode` | 解析 Omron ASCII 回應 |
| OnDataReceived(byte[], int) | protected override | `void` | 處理 Omron 回應（完成碼 + 資料） |
| BuildReadCommand(int page) | private | `byte[]` | 組裝 ASCII 讀取命令（含頁面位元遮罩） |
| BuildWriteCommand(int page, string data) | private | `byte[]` | 組裝 ASCII 寫入命令 |

### IDReaderOmronHex

| 成員 | 存取範圍 | 型別 | 說明 |
|------|---------|------|------|
| CarrierIDReaderType | public override | `CarrierIDReaderType` | 回傳 `OmronHex` |
| GetCarrierID(out string) | public override | `ErrorCode` | HEX 模式讀取流程 |
| SetCarrierID(string) | public override | `ErrorCode` | HEX 模式寫入流程 |
| ParseCarrierIDReaderData(string) | public override | `ErrorCode` | 解析 Omron HEX 回應（hex→ASCII 轉換） |
| OnDataReceived(byte[], int) | protected override | `void` | 處理 Omron 回應 |
| BuildReadCommand(int page) | private | `byte[]` | 組裝 HEX 讀取命令 |
| BuildWriteCommand(int page, string data) | private | `byte[]` | 組裝 HEX 寫入命令 |

### IDReaderHermesRFID

| 成員 | 存取範圍 | 型別 | 說明 |
|------|---------|------|------|
| CarrierIDReaderType | public override | `CarrierIDReaderType` | 回傳 `HermesRFID` |
| GetCarrierID(out string) | public override | `ErrorCode` | Hermes 讀取流程 |
| SetCarrierID(string) | public override | `ErrorCode` | Hermes 寫入流程 |
| ParseCarrierIDReaderData(string) | public override | `ErrorCode` | 解析 Hermes 回應（含校驗碼驗證） |
| OnDataReceived(byte[], int) | protected override | `void` | 處理 Hermes 回應（命令字元解碼 + 校驗碼驗證） |
| PrepareCommand(string message) | private | `byte[]` | 組裝 Hermes 命令框架（含 XOR + 加法校驗碼） |

---

## 狀態轉換

### Reader Operation State

```
IDLE ──[CheckBusy() 成功]──> BUSY
BUSY ──[操作完成/失敗/逾時]──> IDLE (ReleaseBusy)
IDLE ──[CheckBusy() 已忙碌]──> 回傳 CarrierIdBusy (-304)
```

### Hermes 命令狀態機（內部）

```
IDLE ──[發送命令]──> WAITACK
WAITACK ──[收到 'x'/'w'/'v' 回應]──> IDLE (成功)
WAITACK ──[收到 'e' 回應]──> IDLE (錯誤)
WAITACK ──[逾時]──> IDLE (逾時)
```

---

## 資料流

### 讀取操作

```
Caller
  → ICarrierIDReader.GetCarrierID(out carrierID)
    → CarrierIDReader.CheckBusy()
    → [子類別].GetCarrierID()
      → IConnector.Send(命令 bytes)
      → ManualResetEventSlim.Wait(timeout)
      → OnDataReceived(回應 bytes)
      → ParseCarrierIDReaderData(回應)
      → carrierID = ASCII string
    → CarrierIDReader.ReleaseBusy()
  ← ErrorCode + carrierID
```
