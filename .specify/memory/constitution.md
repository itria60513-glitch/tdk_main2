<!--
Sync Impact Report — v3.6.3 → v3.6.4

Version change: 3.6.3 → 3.6.4 (PATCH: IConnector 可置換規則強化為
必須納入模組 interface，讓外部使用者可透過介面替換連接器；範例更新
為同時展示 interface 宣告與 class 實作)

Modified sections:
- 通訊事件訂閱規則: IConnector 可置換規則新增「必須宣告於介面」
  約束；範例擴充為 interface + class 雙段落

Added sections: (none)

Removed sections: (none)

Templates requiring updates:
- .specify/templates/plan-template.md — ✅ no change needed
- .specify/templates/spec-template.md — ✅ no change needed
- .specify/templates/tasks-template.md — ✅ no change needed
- .specify/templates/commands/*.md — ✅ not present
- .github/copilot-instructions.md — ✅ no change needed

Follow-up TODOs:
- None
-->
# TDKService 專案憲章

**版本**: 3.6.4
**批准日期**: 2026-02-01
**最後修訂**: 2026-03-12

## 核心原則

### I. 程式碼品質

所有程式碼**必須**符合以下品質標準：

- **命名慣例**：類別與方法**必須**使用 PascalCase；區域變數**應該**使用 camelCase。
- **單一職責**：每個類別與方法**必須**具有單一職責，避免緊密耦合。
- **程式碼重用**：重複邏輯**必須**提取至共用方法或類別（DRY 原則）。
- **文件**：公開 API 與複雜邏輯**必須**包含以英文撰寫的 XML 文件註解。
- **程式碼註解**：所有實作程式碼**必須**包含全面且詳盡的英文註解，說明：
    - **目的**：程式碼的作用及存在原因
    - **演算法**：複雜邏輯的實作方式（逐步或虛擬碼）
    - **參數**：方法參數的含義與有效範圍
    - **回傳值**：回傳內容及條件
    - **例外**：可能拋出的例外及原因
    - **副作用**：程式碼產生的狀態變更或外部影響
    - **參考**：外部規格、協定或需求的連結
    - **範例**：非顯而易見的方法的使用範例
- **實作細節註解**：對於任何非簡單的邏輯區塊，**必須**包含行內註解說明：
    - 該程式碼區段的目的
    - 為何選擇此特定方法
    - 任何假設或限制
    - 錯誤條件及處理方式
- **錯誤處理**：所有可能失敗的操作**必須**有適當的例外處理。
- **SOLID 原則**：架構**應該**遵循 SOLID 原則，特別是依賴反轉。

#### 建構函式與相依性注入規則

- **Null 檢查注入參數**：建構函式**必須**驗證注入參數，為 null 時拋出 `ArgumentNullException`。
- **儲存為類別成員**：注入的相依性**必須**指派為類別層級屬性或 private readonly 欄位。
- **範例模式**：
  ```csharp
  public class MyService
  {
      private readonly ILogger _logger;
      private readonly IRepository _repository;

      public MyService(ILogger logger, IRepository repository)
      {
          _logger = logger ?? throw new ArgumentNullException(nameof(logger));
          _repository = repository ?? throw new ArgumentNullException(nameof(repository));
      }
  }
  ```

#### 通訊事件訂閱規則

- **使用屬性管理暴露事件的相依性**：當注入的相依性暴露事件需要訂閱時，**必須**使用帶有自訂 setter 的屬性而非 readonly 欄位。
- **IConnector 可置換規則**：模組注入的 `IConnector` **必須**以具有
  public setter 的屬性宣告於模組的介面（interface）中，讓外部使用者
  可透過介面在執行期替換連接器實例。**不得**將 `IConnector` 儲存為
  private readonly 欄位，亦**不得**以 `internal` 修飾符隱藏於介面之外。
- **指派時訂閱**：指派新值到屬性時，**必須**訂閱新實例上所需的事件。
- **重新指派前取消訂閱**：指派新實例前，**必須**取消訂閱舊實例的事件。
- **Null 安全取消訂閱**：嘗試取消訂閱前**必須**檢查 null。
- **組態參數作為公開屬性**：當模組需要組態參數時，組態物件透過建構函式注入，並以公開屬性暴露於介面以供外部存取。
- **範例模式**：
  ```csharp
  // ================ Interface: declare Connector as settable ================
  // IConnector MUST appear in the module interface so that external
  // consumers can replace the connector without knowing the concrete type.
  public interface ILoadPortActor : IDisposable
  {
      // ... other members ...

      /// <summary>
      /// Gets or sets the communication channel to TAS300 hardware.
      /// Replacing the connector at runtime automatically re-wires
      /// the DataReceived event subscription inside the implementation.
      /// </summary>
      IConnector Connector { get; set; }
  }

  // ================ Implementation: property setter pattern ================
  public class LoadportActor : ILoadPortActor
  {
      private IConnector _connector;

      // --------------- IConnector property (public, settable) ---------------
      // Implements ILoadPortActor.Connector.
      // The setter handles unsubscribe/subscribe so callers simply assign
      // a new connector and event routing updates automatically.
      public IConnector Connector
      {
          get => _connector;
          set
          {
              // Step 1: Unsubscribe from old connector (null-safe)
              if (_connector != null)
                  _connector.DataReceived -= OnDataReceived;

              // Step 2: Store the new connector reference
              _connector = value;

              // Step 3: Subscribe to the new connector's events
              if (_connector != null)
                  _connector.DataReceived += OnDataReceived;
          }
      }

      // --------------- Constructor ---------------
      // Inject via constructor; assign through property to trigger
      // subscribe logic from the very first assignment.
      public LoadportActor(
          LoadportActorConfig config,
          IConnector connector)
      {
          Config = config
              ?? throw new ArgumentNullException(nameof(config));

          // Assign through the property to trigger subscribe logic
          Connector = connector
              ?? throw new ArgumentNullException(nameof(connector));
      }

      // --------------- Event handler ---------------
      private void OnDataReceived(
          byte[] byData, int length) { /* Handle */ }
  }
  ```

#### 例外處理與日誌規則

- **Try-catch 保護**：公開與內部方法**必須**使用 try-catch 區塊，避免未處理的例外傳播。
- **記錄回傳值失敗**：當呼叫的方法回傳錯誤或失敗條件時，**必須**記錄該失敗。
- **重新拋出前記錄**：捕捉例外時，在重新拋出或回傳預設值前，記錄詳細資訊（包含方法名稱、參數、例外）。
- **範例模式**：
  ```csharp
  public ResultType DoSomething(ParamType param)
  {
      try
      {
          var result = _dependency.Execute(param);
          return result;
      }
      catch (Exception ex)
      {
          _logger.Error($"DoSomething Failed: {ex.Message}", ex);
          throw;
      }
  }
  ```

#### 程式碼註解與文件指南

- **XML 文件註解**：所有公開方法與屬性**必須**包含 XML 文件註解（`<summary>`、`<param>`、`<returns>`、`<exception>`、`<remarks>`、`<example>`）。
- **實作註解**：所有實作程式碼區塊**必須**包含英文行內註解，說明 What、Why、How、Constraints。
- **協定與規格註解**：實作協定定義行為時，**必須**參考規格文件並說明程式碼與規格的對應關係。

#### 類別設計指南

- **禁止未授權類別**：未經使用者明確核准，**不得**引入新類別。
- **單一檔案模組規則**：每個模組**必須**保持在一個 .cs 檔案中。
- **禁止自行定義類別**：開發者**不得**自行建立新類別。
- **常數偏好使用 enum**：定義一組常數值時，**必須**使用 `enum`。**例外**：錯誤碼遵循統一錯誤碼政策，**必須**使用 `int` 搭配 `const int` 欄位（參見技術限制）。
- **最簡實作**：總是選擇最簡單的實作方式。
- **重用現有介面**：使用專案既有的介面定義。
- **介面優先**：模組互動**必須**透過介面定義。
- **參考既有設計**：實作**必須**參考專案中既有的類別結構與模式。
- **設計權限**：使用者持有架構決策權。

#### YAGNI（You Aren't Gonna Need It）

- **避免投機性功能**：不實作目前不需要的功能或抽象層。
- **不過度工程**：避免不必要的設計模式、包裝類別或抽象。
- **不投機性搭建**：除非明確要求，不添加「未來防護」程式碼。
- **漸進式演進**：僅在需求明確且使用者核准後引入複雜設計。

#### 可讀性要求

- **函式分解**：將複雜流程拆分為具有明確職責的小函式。
- **函式長度**：偏好方法在約 50 行以內。
- **巢狀深度**：避免超過 3 層巢狀。

#### 方法簽章規則

- **參數數量**：方法**不應**超過 4 個參數。
- **避免複雜委派**：避免接受 Action/Delegate/Func 作為參數。
- **明確回傳型別**：方法**必須**有明確的回傳型別。

---

### II. 測試標準

所有功能**必須**有完整的測試覆蓋：

- **框架**：使用 NUnit 進行單元測試，Moq 進行模擬。
- **測試優先**：新功能開發**應該**在可行時遵循 TDD。
- **測試類型**：
    - 單元測試：驗證單一類別或方法的隔離行為。
    - 整合測試：驗證元件間的互動。
    - 契約測試：驗證請求/回應格式。
- **測試命名**：測試方法**必須**遵循 `MethodName_Scenario_ExpectedResult` 模式。
- **模擬**：外部相依性**必須**在單元測試中被模擬。
- **覆蓋率**：所有模組**應該**以 100% 單元測試覆蓋率為目標。核心業務邏輯**必須**達到至少 90% 覆蓋率。
- **介面與類別合規**：測試程式碼**必須**僅針對使用者定義的介面與類別進行測試，**不得**為了提高覆蓋率而自行新增未經授權的輔助類別或介面。
- **模組隔離**：測試**必須**模擬跨層相依性以確保隔離。
- **測試檔案合併**：單一模組/類別的所有測試**必須**放在一個測試檔案中。

---

### III. 使用者體驗一致性

所有使用者介面**必須**提供一致的體驗：

- **UI 語言**：UI 文字**必須**為英文。
- **錯誤訊息**：錯誤訊息**必須**為英文，並清楚說明問題與修復方式。
- **輸入驗證**：所有使用者輸入**必須**被驗證並回傳清楚的驗證錯誤。
- **進度回饋**：長時間操作**必須**提供進度或狀態回饋。

---

### IV. 效能要求

所有功能**必須**滿足效能期望：

- **記憶體使用量**：服務**應該**將記憶體使用量控制在合理範圍內。
- **快取**：頻繁存取的資料**應該**在適當時實作快取。
- **資源釋放**：IDisposable 資源（如 SerialPort、Timer）**必須**正確釋放。

---

## 技術限制

### 技術堆疊

| 項目 | 標準 |
|------|------|
| 框架 | .NET Framework 4.7.2 |
| 測試 | NUnit 3.x + Moq |
| 語言 | C# 7.3 |
| IDE | Visual Studio 2022+ |

### 相依性管理

- 所有相依性**必須**透過 NuGet 管理。
- 新增相依性前**必須**評估其必要性與維護狀態。
- 避免使用未維護的套件。

### 檔案建立政策

- 未經使用者明確要求，**不得**新增檔案。
- 若變更需要新檔案，**必須**停下來等待使用者指示。

### 統一錯誤碼政策

所有 TDKController 模組**必須**使用錯誤碼進行結果回傳，使用統一的範圍式錯誤碼方案。錯誤碼**應該**定義為 `ErrorCode` 列舉或 `int` 型別。

#### ErrorCode 列舉定義

TDKController **應該**定義統一的 `ErrorCode` 列舉以提高型別安全性：

```csharp
public enum ErrorCode : int
{
    Success = 0,              // 操作成功
    Info = 1,                 // 資訊碼（1-99）
    Warning = 100,            // 警告碼（100-199）
    E84Error = -1,            // E84 模組錯誤（-1 至 -99）
    LoadportError = -100,     // LoadportActor 錯誤（-100 至 -199）
    N2PurgeError = -200,      // N2 Purge 錯誤（-200 至 -299）
    CarrierIdError = -300,    // CarrierID Reader 錯誤（-300 至 -399）
    LightCurtainError = -400  // Light Curtain 錯誤（-400 至 -499）
}
```

回傳值的正負號與範圍決定類別：

#### 回傳值範圍

| 範圍 | 類別 | 說明 |
|------|------|------|
| 0 | 成功 | 操作成功完成 |
| 1 ~ 99 | 資訊 | 資訊狀態（少用）|
| 100 ~ 199 | 警告 | 警告條件 |
| < 0 | 錯誤 | 錯誤條件（參見下方模組範圍）|

#### 錯誤碼模組範圍（負值）

| 範圍 | 模組 | 說明 |
|------|------|------|
| -1 ~ -99 | E84 | E84 介面錯誤 |
| -100 ~ -199 | LoadportActor | Driver / Loadport 錯誤 |
| -200 ~ -299 | N2 Purge | N2 Purge 模組錯誤 |
| -300 ~ -399 | CarrierID Reader | CarrierID Reader 模組錯誤 |
| -400 ~ -499 | Light Curtain | Light Curtain 模組錯誤 |

#### 判讀規則

- **成功檢查**：`result == 0` 表示成功。
- **錯誤檢查**：`result < 0` 表示錯誤；範圍識別來源模組。
- **警告檢查**：`result >= 100 && result <= 199` 表示警告，不阻止操作完成。
- **資訊檢查**：`result >= 1 && result <= 99` 表示資訊狀態（少用）。

#### 錯誤碼指南

- **回傳型別**：所有回傳錯誤碼的公開方法**必須**使用 `ErrorCode` 或 `int` 型別。
- **模組隔離**：每個模組**必須**在其分配的負值範圍內嚴格定義錯誤碼。
- **範圍保留**：新模組**必須**在憲章中申請範圍分配後才能定義錯誤碼。
- **帶上下文記錄**：回傳錯誤碼時，**必須**記錄數值與可讀描述以供除錯。
- **保留原始碼**：跨模組邊界傳播錯誤時，**必須**回傳原始負值錯誤碼（不重新映射）。
- **常數檔案**：每個模組**應該**為其錯誤碼定義具名的 `const int` 欄位。

#### 範例

> 錯誤碼常數定義與使用範例將於各模組的規格與實作階段定義。
> 請參考 `specs/` 目錄下各模組的規格文件以獲取具體範例。

### 介面使用政策

- **參考介面穩定性**：使用者提供的參考介面檔案原則上**不得**被修改或拆分為多個檔案。
- **功能限定例外**：僅當使用者對具名 feature 明確批准，且變更對象是該 feature 直接相關的參考介面時，才允許進行最小必要修改。
- **例外範圍限制**：核准例外僅限該 feature 所需成員；**不得**一併調整不相關成員、命名、簽章或檔案結構。
- **文件記錄義務**：任何核准例外**必須**同步記錄於該 feature 的 spec、plan、tasks，並明列核准來源、目標介面、允許修改成員範圍，以及明確排除的不可修改項目。
- **不得自動推廣**：單一 feature 的核准例外**不得**推廣為其他 feature、其他介面或一般性修改許可。
- **IConnector 參考穩定性**：`IConnector` 為專案 communication 定義的核心基礎設施介面，**不得**因任何 feature 例外而修改、替換、拆分或移除。
- **HRESULT 參考穩定性**：`ExceptionManagement.HRESULT` 由參考系統定義，**不得**因任何 feature 例外而修改、替換或模擬。

#### 必要基礎設施介面

- **通訊介面**：`IConnector`（來自 `Communication.Interface` 命名空間）
    - 來源檔案：`Communication/Interface/IConnector.cs`
    - 所有外部通訊（RS232、TCP）**必須**透過此介面

- **日誌介面**：`ILogUtility`（來自 `TDKLogUtility.Module` 命名空間）
    - 來源檔案：`TDKLogUtility/Interface/ILogUtility.cs`
    - TDKLogUtility 專案為**唯讀**，**不得**修改

#### 相依性注入規則

- 通訊與日誌相依性**必須**透過建構函式注入。
- **不得**直接實例化這些介面的具體實作。
- 單元測試**必須**使用 Moq 模擬這些介面。
- 組態（如連接埠號碼、日誌路徑）**必須**外部化。

### 專案組成

| 專案 | 類型 | 說明 | 關鍵介面 |
|------|------|------|----------|
| TDKService | EXE/DLL | 服務層（Host 通訊、命令解析、回應格式化） | Host 協議 |
| TDKController | DLL | 控制器與模組層 | 消費者 |
| TDKLogUtility | DLL | 日誌工具（**唯讀**）| `ILogUtility` |
| Communication | DLL | 傳輸介面（重用既有介面）| `IConnector` |

### 參考檔案管理

- **目錄**：`RefereceFile/`（倉庫根目錄）
- **用途**：包含參考實作、外部介面定義、系統型別定義
- **存取政策**：唯讀 — 未經使用者明確核准**不得**修改

### 標準專案結構

```text
ExampleProject/
├── GUI/
├── Config/
├── Module/
└── Interface/
```

### TDKController 模組

TDKController **必須**包含的模組：

- LoadportActor

#### TDKController 模組方法簽名規則

TDKController 模組中所有控制方法**必須**遵循以下簽名原則：

- **回傳型別**：`ErrorCode`（錯誤碼列舉；0 = 成功，>0 = 警告/資訊，<0 = 錯誤）
- **`out` 參數**：僅在方法需要回傳查詢資料時才使用。動作命令（如 Init、Load）不強制要求 `out` 參數。

**簽名模式**：

```csharp
// 動作命令（無資料回傳）
public ErrorCode MethodName()

// 查詢命令（需回傳資料）
public ErrorCode MethodName(out string data)

// 帶輸入參數的命令
public ErrorCode MethodName(int param)
public ErrorCode MethodName(out string data, int param)
```

| 元素 | 要求 | 說明 |
|------|------|------|
| 回傳型別 | `ErrorCode` | 錯誤碼列舉；0 = 成功，>0 = 警告/資訊，<0 = 錯誤 |
| `out` 參數 | 按需使用 | 僅查詢類方法需要；回傳 TAS300 原始回應或處理後的狀態資料 |
| 輸入參數 | 按需使用 | 遵循方法簽章規則（不超過 4 個參數）|

**特殊規則**：

- 查詢方法的 `out` 參數回傳 Loadport（TAS300）設備原始回應或處理後的狀態資料
- Host 協議層（TDKService）負責將回傳值格式化為 `io <command> <status> {data}\r\n` 格式
- Host 回應碼（如 `0x1`、`0xc017`、`0xc021`）由 TDKService 根據 `ErrorCode` 判斷產生，Module 層與 Controller 層不負責

### 分層規則

```
Service 層（TDKService）— Host 通訊、命令解析、回應格式化
Controller 層（TDKController/Controller）— Facade，組合與協調模組
Module 層（TDKController 內的設備）— 硬體控制模組
Infrastructure 層（TDKLogUtility、Communication、Config）
```

- **TDKService** 為最上層，負責與 Host 通訊（接收命令、發送回應）。
- **TDKService** 呼叫 **TDKController**，TDKController 再呼叫對應的 Module 方法。
- **TDKService** 管理全域程式狀態機（`ProgramState`），控制命令接受與拒絕。
- 上層**可以**注入並使用下層模組。
- 下層**不得**依賴或引用上層。
- 同層模組**應該**透過介面互動。
- Infrastructure 層為所有層共享的基礎。

#### TDKService 程式狀態機

TDKService **必須**管理全域程式狀態機，用於控制命令接受與拒絕：

```csharp
public enum ProgramState
{
    NotInitialized = 0,  // 系統未初始化（Init 尚未成功執行）
    Ready = 1,           // 系統就緒，可接受新命令
    Busy = 2             // 正在執行命令，拒絕新請求
}
```

**狀態轉換規則**：

| 從 | 到 | 觸發條件 |
|----|----|----------|
| NotInitialized | Ready | `Init` 命令成功完成 |
| Ready | Busy | 開始執行任何動作命令 |
| Busy | Ready | 動作命令完成（成功或失敗）|
| Ready / Busy | NotInitialized | 系統關機或 Dispose |

**Busy Guard 模式**：

- TDKService 在派發命令至 TDKController/Module 層之前，**必須**檢查當前狀態。
- 狀態為 `Busy` 時，**必須**拒絕新命令（回傳 Host 回應碼 `0xc021`）。
- 狀態為 `NotInitialized` 時，僅允許 `Init` 命令。
- Module 層（如 LoadportActor）**不負責**程式狀態管理，僅關注硬體通訊。

> 📌 參考來源：`lp204.cc:1071-1074` 的 `prg_NOTINIT(0)` / `prg_READY(1)` / `prg_BUSY(2)` 狀態機。原 C++ 實作置於 CLP 類別，新架構將此職責上移至 TDKService 層。

### 測試專案結構

```text
AutoTest/
├── AutoTest.sln              # 管理所有 UT 專案的 Solution
├── ExampleProject.Tests/
│   ├── Unit/
│   ├── Integration/
│   └── Helpers/
```

- 所有單元測試專案**必須**置於 `AutoTest/` 資料夾底下。
- `AutoTest/AutoTest.sln` 負責管理所有測試專案。
- 主要 Solution（`TDKServiceMiniPC.sln`）不包含測試專案。

### 通訊協定設計

專案支援 TCP 和 RS232 兩種傳輸方式，兩者**必須**透過 `IConnector` 介面遵循統一設計原則。

---

## 文件標準

### 語言要求

- 規格與計畫**必須**以繁體中文（zh-TW）撰寫。
- 使用指南**必須**同時提供繁體中文（zh-TW）與英文（en-US）版本。
- 程式碼註解**必須**以英文撰寫。

### 實作輸出限制

- 實作過程中，註解與標題**不得**新增 speckit 的 task 分類。
- **範例**：
    - ✅ `#region Connector Subscribe/Unsubscribe Tests`
    - ❌ `#region T082: Connector Subscribe/Unsubscribe Tests`

### 文件結構

- 每個功能**必須**有規格文件。
- 複雜功能**必須**包含實作計畫與任務清單。
- API 端點（適用時）**必須**有請求/回應的契約定義。

---

## 治理

### 憲章權威

本憲章為專案的權威指引。所有開發活動**必須**遵守。

### 修訂程序

1. 提出修訂案並附上理由與影響分析。
2. 評估對既有程式碼與文件的影響。
3. 更新相關模板與檔案。
4. 依據語意版本規則調整版本號：
    - MAJOR：不相容的治理/原則變更
    - MINOR：新增原則或實質性擴充
    - PATCH：澄清、措辭修正、非語意改進

### 合規檢查

- 所有程式碼審查**必須**驗證是否符合憲章。
- 任何偏離憲章的行為**必須**被記錄並核准。
- 已核准的 feature 介面例外**必須**驗證 spec、plan、tasks 三者對目標介面、允許成員範圍與不可修改項目之記錄完全一致。
- 定期審查憲章並在必要時提出修訂。
