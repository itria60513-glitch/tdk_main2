# 快速入門：Carrier ID Reader Implementation

**Branch**: `002-carrier-id-reader-impl` | **Date**: 2026-03-10

## 概覽

CarrierID Reader 模組提供統一的載體識別介面，支援四種讀取器類型。

## 使用方式

### 1. 建立讀取器實例

```csharp
// Step 1: Prepare dependencies
var config = new CarrierIDReaderConfig
{
    ReaderType = CarrierIDReaderType.BarcodeReader,
    TimeoutMs = 10000,
    MaxRetryCount = 3
};
IConnector connector = /* ... injected communication connector ... */;
ILogUtility logger = /* ... injected log utility ... */;

// Step 2: Create reader (choose one based on configured type)
ICarrierIDReader reader = new IDReaderBarcodeReader(config, connector, logger);
// or: new IDReaderOmronASCII(config, connector, logger);
// or: new IDReaderOmronHex(config, connector, logger);
// or: new IDReaderHermesRFID(config, connector, logger);
```

### 2. 讀取載體 ID

```csharp
ErrorCode result = reader.GetCarrierID(out string carrierID);
if (result == ErrorCode.Success)
{
    // carrierID contains the ASCII string identifier
    logger.WriteLog("Reader", "Carrier ID: " + carrierID);
}
else
{
    logger.WriteLog("Reader", "Read failed: " + result.ToString());
}
```

### 3. 寫入載體 ID（僅 RFID）

```csharp
ErrorCode result = reader.SetCarrierID("CARRIER_DATA_001");
if (result == ErrorCode.Success)
{
    logger.WriteLog("Reader", "Write succeeded");
}
```

## 錯誤處理

| ErrorCode | 處理建議 |
|-----------|---------|
| Success (0) | 操作成功 |
| CarrierIdTimeout (-302) | 檢查裝置連線、增加逾時值 |
| CarrierIdBusy (-304) | 等待前一操作完成後重試 |
| CarrierIdReadFailed (-312) | 檢查載體媒體是否就位 |
| CarrierIdConnectFailed (-309) | 檢查通訊埠設定 |
| CarrierIdChecksumError (-308) | 檢查 Hermes 讀取器線路品質 |

## 建置與測試

```powershell
# 建置專案
msbuild TDKServer.sln /p:Configuration=Debug

# 執行單元測試
dotnet test AutoTest/TDKController.Tests/TDKController.Tests.csproj
```

## 架構摘要

```
ICarrierIDReader (介面)
       │
CarrierIDReader (基底，abstract)
       │
  ┌────┼────────┬────────────┐
  │    │        │            │
BarcodeReader  OmronASCII  OmronHex  HermesRFID
```

每個子類別負責其設備專屬的協定流程，基底類別處理共用邏輯（並行保護、通訊管理、日誌）。
