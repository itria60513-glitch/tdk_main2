# 介面合約：ICarrierIDReader

**Branch**: `002-carrier-id-reader-impl` | **Date**: 2026-03-10

## 介面定義

```csharp
namespace TDKController.Interface
{
    /// <summary>
    /// Interface for carrier ID reader operations.
    /// Provides a unified contract for reading and writing carrier identifiers
    /// across different reader types (Barcode, Omron ASCII, Omron HEX, Hermes RFID).
    /// </summary>
    public interface ICarrierIDReader
    {
        /// <summary>
        /// Gets or sets the configuration file path for the carrier ID reader.
        /// </summary>
        string CarrierIDReaderConfigPath { get; set; }

        /// <summary>
        /// Gets the type of the carrier ID reader.
        /// </summary>
        CarrierIDReaderType CarrierIDReaderType { get; }

        /// <summary>
        /// Parses raw device response data and updates internal state.
        /// </summary>
        /// <param name="command">Raw device response string to parse.</param>
        /// <returns>
        /// ErrorCode.Success (0) if parsing succeeds.
        /// Negative ErrorCode (-300 range) if parsing fails.
        /// </returns>
        ErrorCode ParseCarrierIDReaderData(string command);

        /// <summary>
        /// Reads the carrier identifier from the configured device.
        /// Executes the full protocol-specific read sequence.
        /// </summary>
        /// <param name="carrierID">
        /// When successful, contains the carrier identifier as an ASCII string.
        /// When failed, contains null or empty string.
        /// </param>
        /// <returns>
        /// ErrorCode.Success (0) if the read operation succeeds.
        /// ErrorCode.CarrierIdBusy (-304) if another operation is in progress.
        /// ErrorCode.CarrierIdTimeout (-302) if the device does not respond within the timeout.
        /// ErrorCode.CarrierIdCommandFailed (-303) if the device rejects the command.
        /// Other negative ErrorCode (-300 range) for specific failures.
        /// </returns>
        ErrorCode GetCarrierID(out string carrierID);

        /// <summary>
        /// Writes the carrier identifier to the configured RFID device.
        /// Only supported by RFID reader types (Omron ASCII, Omron HEX, Hermes RFID).
        /// </summary>
        /// <param name="carrierID">The carrier identifier string to write.</param>
        /// <returns>
        /// ErrorCode.Success (0) if the write operation succeeds.
        /// ErrorCode.CarrierIdBusy (-304) if another operation is in progress.
        /// ErrorCode.CarrierIdCommandFailed (-303) if the device rejects the write.
        /// ErrorCode.CarrierIdError (-300) if the reader type does not support write.
        /// Other negative ErrorCode (-300 range) for specific failures.
        /// </returns>
        ErrorCode SetCarrierID(string carrierID);
    }
}
```

## 方法合約

### GetCarrierID

| 條件 | 預期行為 |
|------|---------|
| 正常讀取（設備在線、載體存在） | 回傳 `Success`，`carrierID` 為有效 ASCII 字串 |
| 設備逾時 | 回傳 `CarrierIdTimeout`，`carrierID` 為 null |
| 設備拒絕命令 | 回傳 `CarrierIdCommandFailed`，`carrierID` 為 null |
| 另一操作進行中 | 回傳 `CarrierIdBusy`，`carrierID` 為 null（不發送命令） |
| Barcode 重試耗盡 | 回傳 `CarrierIdReadFailed`，`carrierID` 為 null |
| 連線失敗 | 回傳 `CarrierIdConnectFailed`，`carrierID` 為 null |

### SetCarrierID

| 條件 | 預期行為 |
|------|---------|
| RFID 正常寫入 | 回傳 `Success` |
| Barcode 呼叫 SetCarrierID | 回傳 `CarrierIdError`（不支援寫入） |
| 設備逾時 | 回傳 `CarrierIdTimeout` |
| 設備拒絕命令 | 回傳 `CarrierIdCommandFailed` |
| 另一操作進行中 | 回傳 `CarrierIdBusy`（不發送命令） |
| 無效參數（長度不對等） | 回傳 `CarrierIdInvalidParameter` |

## 列舉合約

### CarrierIDReaderType

```csharp
namespace TDKController
{
    /// <summary>
    /// Supported carrier ID reader device types.
    /// </summary>
    public enum CarrierIDReaderType
    {
        /// <summary>Barcode reader (BL600 series).</summary>
        BarcodeReader = 0,

        /// <summary>Omron RFID reader in ASCII content format.</summary>
        OmronASCII = 1,

        /// <summary>Omron RFID reader in HEX content format.</summary>
        OmronHex = 2,

        /// <summary>Hermes RFID reader.</summary>
        HermesRFID = 3
    }
}
```

## 錯誤碼合約

```csharp
// 新增至 TDKController.ErrorCode 列舉（-300 ~ -399 範圍）

/// <summary>Base Carrier ID Reader error.</summary>
CarrierIdError = -300,

/// <summary>Signal reset failed during reader operation.</summary>
CarrierIdSemaphoreResetFailed = -301,

/// <summary>Device response timeout (default 10 seconds).</summary>
CarrierIdTimeout = -302,

/// <summary>Device reported command failure (NAK, error code, etc.).</summary>
CarrierIdCommandFailed = -303,

/// <summary>Reader is busy with a previous operation.</summary>
CarrierIdBusy = -304,

/// <summary>Invalid RFID page number.</summary>
CarrierIdInvalidPage = -305,

/// <summary>Invalid parameter (e.g., wrong data length for write).</summary>
CarrierIdInvalidParameter = -306,

/// <summary>Response data exceeds buffer capacity.</summary>
CarrierIdResponseTooLong = -307,

/// <summary>Checksum verification failed (Hermes RFID).</summary>
CarrierIdChecksumError = -308,

/// <summary>Failed to establish connection to reader device.</summary>
CarrierIdConnectFailed = -309,

/// <summary>BarcodeReader MotorON command failed.</summary>
CarrierIdMotorOnFailed = -310,

/// <summary>BarcodeReader MotorOFF command failed.</summary>
CarrierIdMotorOffFailed = -311,

/// <summary>Read operation failed after all retries exhausted.</summary>
CarrierIdReadFailed = -312,

/// <summary>Internal error (frame assembly, etc.).</summary>
CarrierIdInternalError = -313
```
