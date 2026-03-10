# 實作計畫：Carrier ID Reader Implementation

**Branch**: `002-carrier-id-reader-impl` | **Date**: 2026-03-10 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-carrier-id-reader-impl/spec.md`

## 摘要

實作 TDKController 中的 CarrierID Reader 模組，支援四種讀取器類型（Barcode BL600、Omron ASCII RFID、Omron HEX RFID、Hermes RFID），以繼承架構設計提供統一的 `ICarrierIDReader` 介面。基底類別 `CarrierIDReader` 實作共用邏輯（組態、通訊、日誌、並行保護），四個子類別各自實作設備專屬的協定流程。讀取結果統一回傳 ASCII 字串，錯誤碼使用 -300 ~ -399 範圍。

## 技術脈絡

**Language/Version**: C# 7.3 / .NET Framework 4.7.2  
**Primary Dependencies**: `IConnector`（Communication 專案）、`ILogUtility`（TDKLogUtility 專案，唯讀）  
**Storage**: N/A（無持久化需求）  
**Testing**: NUnit 3.x + Moq  
**Target Platform**: Windows 桌面（Windows Forms 主機應用程式）  
**Project Type**: 類別庫（TDKController.dll 內的模組）  
**Performance Goals**: 單次讀寫操作 ≤ 10 秒（逾時上限）  
**Constraints**: .NET Framework 4.7.2 限制（無 async/await Task-based patterns 預設使用）；C# 7.3 語法限制  
**Scale/Scope**: 4 個讀取器子類別 + 1 個基底類別 + 1 個介面 + 1 個組態類別 + 1 個列舉

## 憲章合規檢查

*GATE: 必須在 Phase 0 研究前通過。Phase 1 設計後重新檢查。*

| 憲章規則 | 合規狀態 | 說明 |
|----------|---------|------|
| 命名慣例 (PascalCase/camelCase) | ✅ PASS | 類別用 PascalCase，區域變數用 camelCase |
| 單一職責 | ✅ PASS | 基底類別：共用邏輯；子類別：協定專屬邏輯 |
| 介面優先 | ✅ PASS | 透過 `ICarrierIDReader` 介面互動 |
| 禁止未授權類別 | ✅ PASS | 僅建立 spec 中明確要求的類別（CarrierIDReader 基底 + 4 個子類別） |
| 單一檔案模組規則 | ✅ PASS | 每個類別一個 .cs 檔案（使用者明確要求） |
| 建構函式 Null 檢查 | ✅ PASS | 注入參數使用 `?? throw new ArgumentNullException` |
| 通訊事件訂閱規則 | ✅ PASS | `IConnector` 透過屬性管理，含訂閱/取消訂閱 |
| 錯誤碼範圍 (-300 ~ -399) | ✅ PASS | CarrierID Reader 使用分配的負值範圍 |
| 方法簽章規則 (ErrorCode 回傳) | ✅ PASS | 所有公開方法回傳 `ErrorCode`，查詢用 `out` 參數 |
| 分層規則 | ✅ PASS | Module 層僅負責硬體通訊，不管理程式狀態 |
| XML 文件註解 | ✅ PASS | 公開 API 必含 XML 文件 |
| 程式碼實作註解 | ✅ PASS | 所有實作區塊含英文行內註解 |
| 測試標準 (NUnit + Moq) | ✅ PASS | 單元測試模擬 IConnector 與 ILogUtility |
| 介面使用政策 | ✅ PASS | 重用現有 IConnector；不修改參考介面 |
| YAGNI | ✅ PASS | 僅實作 spec 中要求的功能 |
| 檔案建立政策 | ✅ PASS | 使用者已明確要求建立 4 個子類別檔案 |
| 實作輸出限制 | ✅ PASS | 不使用 speckit task 分類作為程式碼註解 |

**GATE 結果**: ✅ 全部通過，無違規項目。
**Phase 1 設計後重新檢查**: ✅ 全部通過（2026-03-10）。設計決策完全符合憲章規則。

## 專案結構

### 文件（本功能）

```text
specs/002-carrier-id-reader-impl/
├── plan.md              # 本檔案
├── research.md          # Phase 0 輸出
├── data-model.md        # Phase 1 輸出
├── quickstart.md        # Phase 1 輸出
├── contracts/           # Phase 1 輸出
└── tasks.md             # Phase 2 輸出（/speckit.tasks）
```

### 原始碼（倉庫根目錄）

```text
TDKController/
├── Interface/
│   ├── ICarrierIDReader.cs          # 介面定義（已存在，待填充）
│   └── ErrorCode.cs                 # 錯誤碼列舉（已存在，需新增 -300 系列）
├── Config/
│   └── CarrierIDReaderConfig.cs     # 組態類別（已存在，待填充）
├── Module/
│   ├── CarrierIDReader.cs           # 基底類別（已存在，待填充）
│   ├── IDReaderBarcodeReader.cs     # 新建：Barcode BL600 子類別
│   ├── IDReaderOmronASCII.cs        # 新建：Omron ASCII RFID 子類別
│   ├── IDReaderOmronHex.cs          # 新建：Omron HEX RFID 子類別
│   └── IDReaderHermesRFID.cs        # 新建：Hermes RFID 子類別

AutoTest/
└── TDKController.Tests/
    └── Unit/
        ├── CarrierIDReaderBaseTests.cs       # 基底類別測試
        ├── IDReaderBarcodeReaderTests.cs     # BarcodeReader 測試
        ├── IDReaderOmronASCIITests.cs        # OmronASCII 測試
        ├── IDReaderOmronHexTests.cs          # OmronHex 測試
        └── IDReaderHermesRFIDTests.cs        # HermesRFID 測試
```

**結構決策**: 遵循既有 TDKController 標準模組結構（Interface/ + Config/ + Module/）。子類別檔案放在 Module/ 目錄下，與使用者要求一致。不建立 `CarrierIDReader/` 子目錄分支（已存在但為空白佔位，改用 Module/ 統一管理）。

## 複雜度追蹤

> 無憲章違規項目，無需填寫。
