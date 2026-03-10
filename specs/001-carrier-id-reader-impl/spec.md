# 功能規格：CarrierIDReader 子類別實作

**功能分支**: `001-carrier-id-reader-impl`
**建立日期**: 2026-03-10
**狀態**: 草稿
**輸入**: 使用者描述：「實作 TDKController CarrierIDReader 子類別：IDReaderBarcodeReader、IDReaderOmronASCII、IDReaderOmronHex、IDReaderHermesRFID，基於 ICarrierIDReader 介面、PlantUML 設計圖及舊版 C++ 參考程式碼 (lp204.cc)」

## 使用者情境與測試 *(必要)*

### 使用者故事 1 — 透過條碼讀取器讀取 Carrier ID（優先級：P1）

EFEM 系統操作員在配備 BL600 系列條碼讀取器的 Load Port 上觸發 Carrier ID 讀取操作。系統開啟條碼讀取器馬達、嘗試掃描條碼（逾時或 NG 結果時最多重試 8 次）、解析掃描資料、關閉馬達，並將 Carrier ID 字串回傳給呼叫端。

**優先級理由**：條碼讀取是最常見的 Carrier ID 識別方式；沒有它系統無法識別 Load Port 上進來的 Carrier。

**獨立測試**：可透過建立 `IDReaderBarcodeReader` 實例、連接通訊通道（或 Mock），呼叫 `GetCarrierID()` 並驗證回傳的 Carrier ID 字串是否符合預期條碼值來完整測試。

**驗收情境**：

1. **假設** 條碼讀取器已連接且有效條碼的 Carrier 已就位，**當** 呼叫 `GetCarrierID()`，**則** 系統發送 MotorON、讀取條碼（最多重試 8 次）、發送 MotorOFF，並回傳 Carrier ID 與成功碼 (0)。
2. **假設** 條碼讀取器已連接但無 Carrier 或條碼無法辨識，**當** 8 次重試全部回傳 NG 或逾時，**則** 系統發送 MotorOFF 並回傳非零錯誤碼表示讀取失敗。
3. **假設** 條碼讀取器已連接，**當** 呼叫 `GetCarrierID()` 且 MotorON 命令失敗，**則** 系統回傳非零錯誤碼，不嘗試讀取。

---

### 使用者故事 2 — 透過 Hermes RFID 讀取器讀取 Carrier ID（優先級：P1）

EFEM 系統使用 Hermes 協定 RFID 讀取器從 RFID 標籤讀取 Carrier ID。系統發送包含頁面/位址資訊的讀取命令、驗證回應校驗碼、解析回傳的十六進位編碼資料，並回傳 Carrier ID。

**優先級理由**：Hermes RFID 是半導體廠房中廣泛部署的 Carrier ID 讀取器類型，與條碼讀取同等重要。

**獨立測試**：可透過建立 `IDReaderHermesRFID` 實例、連接通訊通道（或 Mock），呼叫 `GetCarrierID()` 並驗證回傳的 Carrier ID 是否符合預期 RFID 標籤內容來測試。

**驗收情境**：

1. **假設** Hermes RFID 讀取器已連接且有效 RFID 標籤的 Carrier 已就位，**當** 呼叫 `GetCarrierID()`，**則** 系統發送 Hermes 讀取命令、收到確認成功回應及標籤資料、解析資料，並回傳 Carrier ID 與成功碼 (0)。
2. **假設** Hermes RFID 讀取器已連接但無標籤或無回應，**當** 呼叫 `GetCarrierID()`，**則** 系統逾時或收到錯誤確認，回傳非零錯誤碼並記錄失敗碼。
3. **假設** Hermes RFID 讀取器已連接，**當** 以有效的 16 字元十六進位字串與頁碼呼叫 `SetCarrierID()`，**則** 系統發送 Hermes 寫入命令，收到確認成功回應後回傳成功 (0)。

---

### 使用者故事 3 — 透過 Omron RFID 讀取器讀取 Carrier ID（ASCII 模式）（優先級：P2）

系統從以 ASCII 內容格式運作的 Omron RFID 讀取器讀取 Carrier ID。讀取命令指定頁碼（1–30），將內容格式設為 ASCII（一次讀取兩個相鄰頁面），並解析回傳的 ASCII 字串作為 Carrier ID。

**優先級理由**：Omron ASCII 模式是次要的 RFID 讀取器變體；系統必須支援以保持硬體彈性，但使用頻率低於條碼或 Hermes。

**獨立測試**：可透過建立 `IDReaderOmronASCII` 實例、連接 Mock 通訊通道，呼叫 `GetCarrierID()` 並驗證回應是否符合已知的 ASCII 格式 RFID 內容來測試。

**驗收情境**：

1. **假設** ASCII 模式的 Omron RFID 讀取器已連接且有標籤，**當** 呼叫 `GetCarrierID()`，**則** 系統發送內容格式位元組 = '1' (ASCII) 的 Omron 讀取命令、收到完成碼 "00" 及資料，並回傳解析後的 Carrier ID 與成功碼 (0)。
2. **假設** ASCII 模式的 Omron 讀取器已連接但未偵測到標籤，**當** 呼叫 `GetCarrierID()`，**則** 系統在 3 秒後逾時並回傳非零錯誤碼。
3. **假設** ASCII 模式的 Omron 讀取器，**當** 以有效資料與頁碼呼叫 `SetCarrierID()`，**則** 系統發送寫入命令，收到 "00" 完成碼後回傳成功。

---

### 使用者故事 4 — 透過 Omron RFID 讀取器讀取 Carrier ID（HEX 模式）（優先級：P2）

系統從以 HEX 內容格式運作的 Omron RFID 讀取器讀取 Carrier ID。讀取命令讀取單一頁面的十六進位資料，系統回傳原始十六進位內容作為 Carrier ID。

**優先級理由**：Omron HEX 模式功能上與 ASCII 模式類似，但讀取單頁十六進位資料；它完善了所有支援的 Omron 讀取器格式。

**獨立測試**：可透過建立 `IDReaderOmronHex` 實例、連接 Mock 通訊通道，呼叫 `GetCarrierID()` 並驗證回應是否符合已知的 HEX 格式 RFID 內容來測試。

**驗收情境**：

1. **假設** HEX 模式的 Omron RFID 讀取器已連接且有標籤，**當** 呼叫 `GetCarrierID()`，**則** 系統發送內容格式位元組 = '0' (HEX) 的 Omron 讀取命令、收到 "00" 完成碼及十六進位資料，並回傳 Carrier ID 與成功碼 (0)。
2. **假設** HEX 模式的 Omron 讀取器已連接但通訊失敗，**當** 呼叫 `GetCarrierID()`，**則** 系統在 3 秒後逾時並回傳非零錯誤碼。

---

### 使用者故事 5 — 統一的 Carrier ID 讀取器介面（優先級：P1）

EFEM 控制器根據組態設定（讀取器類型）建立對應的 `CarrierIDReader` 子類別。無論使用哪個子類別，呼叫端僅透過 `ICarrierIDReader` 介面互動 — 呼叫 `GetCarrierID()`、`SetCarrierID()` 或 `ParseCarrierIDReaderData()` — 無需知道底層讀取器硬體類型。

**優先級理由**：多型介面是架構基礎；沒有它，新增或切換讀取器類型需要修改呼叫端程式碼。

**獨立測試**：可透過建立各子類別實例、轉型為 `ICarrierIDReader`，並驗證所有介面方法皆可呼叫且回傳預期的結果型別來測試。

**驗收情境**：

1. **假設** CarrierIDReader 組態指定條碼讀取器類型，**當** 系統建立 ID 讀取器實例，**則** 回傳的物件實作 `ICarrierIDReader` 且型別為 `IDReaderBarcodeReader`。
2. **假設** CarrierIDReader 組態指定 Hermes RFID 讀取器類型，**當** 系統建立 ID 讀取器實例，**則** 回傳的物件實作 `ICarrierIDReader` 且型別為 `IDReaderHermesRFID`。
3. **假設** 任何 `ICarrierIDReader` 實例，**當** 呼叫 `GetCarrierID()`，**則** 結果遵循相同的回傳碼慣例（0 = 成功，非零 = 特定錯誤），與底層讀取器類型無關。

---

### 邊界情境

- 操作進行中通訊中斷（如條碼掃描或 RFID 讀取時纜線斷開）會發生什麼？
- 從 Hermes RFID 讀取器收到校驗碼無效的回應時系統如何處理？
- Omron RFID 讀取器回傳超過預期緩衝區大小的回應時會發生什麼？
- 條碼讀取器 8 次重試全部回傳 "NG" 時如何處理？
- 以不符合 Hermes/Omron 寫入器預期 16 字元長度的資料呼叫 `SetCarrierID()` 時會發生什麼？
- 前一個命令仍在等待回應（忙碌狀態）時發出讀取或寫入命令，系統如何回應？
- 組態的讀取器類型與實際連接的硬體不符時會發生什麼？

## 需求 *(必要)*

### 功能需求

- **FR-001**: 每個讀取器子類別（IDReaderBarcodeReader、IDReaderOmronASCII、IDReaderOmronHex、IDReaderHermesRFID）**必須**實作 `ICarrierIDReader` 介面並繼承 `CarrierIDReader` 基底類別。
- **FR-002**: `IDReaderBarcodeReader.GetCarrierID()` **必須**發送 MotorON 命令，然後嘗試讀取條碼最多 8 次（NG/逾時時重試），最後發送 MotorOFF 命令後回傳。
- **FR-003**: `IDReaderBarcodeReader` **必須**公開類別圖中定義的 `MotorON()`、`ReadBarCode()` 及 `MotorOFF()` 方法。
- **FR-004**: `IDReaderOmronASCII.GetCarrierID()` **必須**以內容格式設為 ASCII ('1') 發送 Omron 讀取命令，每次操作讀取兩個相鄰頁面。
- **FR-005**: `IDReaderOmronHex.GetCarrierID()` **必須**以內容格式設為 HEX ('0') 發送 Omron 讀取命令，每次操作讀取單一頁面。
- **FR-006**: 兩個 Omron 子類別**必須**根據請求的頁碼（1–30）建構具有正確頁面規格位元遮罩的 13 位元組讀取命令框架。
- **FR-007**: 兩個 Omron 子類別**必須**支援透過 `SetCarrierID()` 進行寫入操作，以 16 字元資料酬載建構 29 位元組寫入命令框架。
- **FR-008**: `IDReaderHermesRFID.GetCarrierID()` **必須**發送具有正確框架（起始位元組、長度、命令、位址、校驗碼）的 Hermes 讀取命令、等待確認，並解析確認成功的回應資料。
- **FR-009**: `IDReaderHermesRFID.SetCarrierID()` **必須**發送具有正確框架及 16 字元資料酬載的 Hermes 寫入命令，收到確認成功回應後回傳成功。
- **FR-010**: 所有讀取器子類別**必須**回傳 `ErrorCode` 列舉值，遵循 TDKController 統一慣例：`ErrorCode.Success`（0）表示成功；CarrierID 範圍負值（-300 至 -399）表示特定失敗條件（`CarrierIdBusy`、`CarrierIdTimeout`、`CarrierIdCommError`、`CarrierIdReadFailed`、`CarrierIdChecksumError`、`CarrierIdDataTooLong`、`CarrierIdMotorError`）。回傳資料的查詢方法（`GetCarrierID`、`ParseCarrierIDReaderData`）**必須**使用 `out string` 參數傳遞結果，遵循 `ILoadPortActor.GetLPStatus(out string data)` 建立的模式。
- **FR-011**: 所有讀取器子類別**必須**使用透過 `CarrierIDReader` 基底類別建構函式提供的通訊通道（connector）來發送與接收資料。
- **FR-012**: 所有讀取器子類別**必須**使用透過 `CarrierIDReader` 基底類別建構函式提供的日誌工具來記錄重要操作（已發送的命令、收到的回應、遇到的錯誤）。
- **FR-013**: 所有讀取器子類別在忙碌時（前一個命令仍在等待回應）**必須**拒絕操作，回傳「忙碌」錯誤碼。
- **FR-014**: 所有讀取器子類別**必須**實作 `ParseCarrierIDReaderData()` 以從各自協定特定的原始回應資料中擷取 Carrier ID。
- **FR-015**: `IDReaderHermesRFID` **必須**在接受回應資料前驗證回應校驗碼（XOR 與加法校驗碼）。
- **FR-016**: 所有與設備通訊的讀取器操作**必須**實作逾時機制（RFID 讀取器 3 秒、條碼讀取器馬達命令最多 10 秒、條碼讀取 5 秒）以防止無限期阻塞。

### 關鍵實體

- **ICarrierIDReader**: 定義所有 Carrier ID 讀取器類型契約的介面。屬性：`CarrierIDReaderConfigPath`（組態檔路徑）、`carrierIDReaderType`（識別讀取器硬體類型的列舉）。方法簽章：`ErrorCode GetCarrierID(out string carrierId)`、`ErrorCode SetCarrierID(string carrierId, int page)`、`ErrorCode ParseCarrierIDReaderData(byte[] rawData, out string carrierId)`（回傳型別均為 `ErrorCode`；有資料輸出的方法使用 `out string` 參數，與 `ILoadPortActor.GetLPStatus(out string data)` 模式一致）。
- **CarrierIDReader**: 提供共用建構邏輯的基底類別（接受 connector/通訊通道與 logger）。所有子類別繼承自此。
- **IDReaderBarcodeReader**: BL600 系列條碼讀取器的子類別。額外方法：`MotorON()`、`ReadBarCode()`、`MotorOFF()`。使用「掃描 + 重試」讀取流程。
- **IDReaderOmronASCII**: ASCII 內容格式 Omron RFID 讀取器的子類別。使用頁面式位元遮罩命令框架，每次操作讀取兩個相鄰頁面。
- **IDReaderOmronHex**: HEX 內容格式 Omron RFID 讀取器的子類別。使用頁面式位元遮罩命令框架，每次操作讀取單一頁面。
- **IDReaderHermesRFID**: Hermes 協定 RFID 讀取器的子類別。使用帶有起始位元組、長度、命令碼、位址、XOR/加法校驗碼的框架命令，並處理確認回應。
- **CarrierIDReaderType**: 識別讀取器硬體類型的列舉，定義於 `TDKController.Interface`。成員：`BarcodeReader = 1`、`HermesRFID = 2`、`OmronASCII = 3`、`OmronHex = 4`。

## 假設

- 通訊通道（connector）抽象已存在於專案中，處理序列埠的開啟/關閉/發送/接收操作。
- 日誌工具抽象已存在，支援寫入帶有可選資料酬載的日誌訊息。
- `CarrierIDReaderConfig` 類別將儲存讀取器類型、通訊埠設定及組態檔路徑等組態。
- Omron 讀取器頁面規格位元遮罩邏輯遵循舊版 C++ `COmron::ReadRFID` 與 `COmron::WriteRFID` 方法中可見的相同演算法。
- Hermes 協定框架（起始位元組 'S'、長度編碼、XOR 校驗碼、加法校驗碼）遵循舊版 C++ `CHermos::prepCmd` 方法。
- 每個子類別檔案置於 `TDKController/CarrierIDReader/Module/` 下作為獨立的 .cs 檔案。

## 成功標準 *(必要)*

### 可量測成果

- **SC-001**: 四個子類別檔案全部編譯無錯誤，且每個類別正確實作 `ICarrierIDReader` 介面。
- **SC-002**: 每個子類別的單元測試 100% 通過，涵蓋各讀取器類型的主要讀取流程（GetCarrierID）與寫入流程（SetCarrierID）。
- **SC-003**: 條碼讀取器子類別在 `GetCarrierID()` 中正確執行重試迴圈（最多 8 次）與馬達開/關序列。
- **SC-004**: 兩個 Omron 子類別對所有有效頁碼（讀取 1–30、寫入 1–17）正確建構頁面規格位元遮罩。
- **SC-005**: Hermes RFID 子類別正確框架命令並計算校驗碼，且驗證回應校驗碼。
- **SC-006**: 所有呼叫端程式碼可透過 `ICarrierIDReader` 介面使用，無需知道具體子類別型別，實現透過組態切換讀取器類型。
