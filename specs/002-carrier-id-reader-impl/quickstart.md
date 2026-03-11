# 快速入門：Carrier ID Reader Implementation

**Branch**: `002-carrier-id-reader-impl` | **Date**: 2026-03-11

## zh-TW

### 建立讀取器

```csharp
var config = new CarrierIDReaderConfig
{
    ReaderType = CarrierIDReaderType.BarcodeReader,
    TimeoutMs = 10000,
    MaxRetryCount = 8,
    Page = 1
};

IConnector connector = /* injected connector */;
ILogUtility logger = /* injected logger */;
ICarrierIDReader reader = new IDReaderBarcodeReader(config, connector, logger);
```

### 讀取流程

```csharp
ErrorCode result = reader.GetCarrierID(out string carrierID);
if (result == ErrorCode.Success)
{
    logger.WriteLog("Reader", "Carrier ID: " + carrierID);
}
```

Barcode BL600 會在單次等待最多 10 秒的前提下執行最多 8 次讀取嘗試。每次呼叫結束時都會執行 trigger off、motor off、disconnect 清理。

### 寫入流程

```csharp
config.ReaderType = CarrierIDReaderType.OmronASCII;
ICarrierIDReader reader = new IDReaderOmronASCII(config, connector, logger);
ErrorCode writeResult = reader.SetCarrierID("CARRIERDATA0001");
```

RFID 寫入會先驗證頁碼、長度與字元格式，不合法 payload 會在送出裝置命令前直接失敗。

## en-US

### Create a reader

```csharp
var config = new CarrierIDReaderConfig
{
    ReaderType = CarrierIDReaderType.OmronHex,
    TimeoutMs = 10000,
    MaxRetryCount = 8,
    Page = 2
};

ICarrierIDReader reader = new IDReaderOmronHex(config, connector, logger);
```

### Read a carrier ID

```csharp
ErrorCode result = reader.GetCarrierID(out string carrierID);
if (result == ErrorCode.Success)
{
    logger.WriteLog("Reader", "Carrier ID: " + carrierID);
}
```

Each device wait stage uses its own 10-second timeout budget. Barcode retries stop immediately on the first valid read and fail after the eighth unreadable result.

### Write a carrier ID

```csharp
ICarrierIDReader reader = new IDReaderHermesRFID(config, connector, logger);
ErrorCode result = reader.SetCarrierID("4142434445463031");
```

Supported RFID writers reject invalid page numbers and malformed payloads before any device I/O is sent.
