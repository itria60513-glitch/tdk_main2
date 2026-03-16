# Feature Specification: Carrier ID Reader Implementation

**Feature Branch**: `002-carrier-id-reader-impl`  
**Created**: 2026-03-10  
**Status**: Draft  
**Input**: User description: "我要實作 TDKController CarrierIDReader中的檔案，請參考 CarrierIDReader.plantuml 來實作，其中 class IDReaderBarcodeReader, IDReaderOmronASCII, IDReaderOmronHex, IDReaderHermesRFID 分別建置一個檔案來實作。IDReaderBarcodeReader 請參考 BarcodeReader-Read.puml、BarcodeReader.puml、lp204.cc:7664-8043。IDReaderOmronASCII 請參考 OmronASCII.puml、OmronASCII-Read、lp204.cc:8695-9194 中 ASCII 的流程。IDReaderOmronHex 請參考 OmronHex.puml、OmronHex-Read.puml、lp204.cc:8695-9194 中 HEX 的流程。IDReaderHermesRFID 請參考 HermesRFID-Read.puml、HermesRFID.puml、lp204.cc:8048-8772。"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Read Carrier ID from the configured barcode reader (Priority: P1)

As an equipment controller, I need to read a carrier identifier from a Keyence barcode reader so that the system can perform carrier identification through the first and most common device type, establishing the end-to-end read workflow.

**Why this priority**: Carrier identification is a prerequisite for downstream routing, validation, and traceability. The barcode reader is the most commonly deployed device type and serves as the MVP read path.

**Independent Test**: Configure the barcode reader, trigger a read request with valid carrier media present, and confirm that the controller returns a usable carrier identifier and a success result. Also verify unreadable media and timeout scenarios.

**Acceptance Scenarios**:

1. **Given** the controller is configured for the barcode reader and valid carrier media is present, **When** a read is requested, **Then** the system returns the carrier identifier and reports success.
2. **Given** the controller is configured for the barcode reader but the carrier media cannot be read, **When** a read is requested, **Then** the system reports a read failure after exhausting up to 8 read attempts without returning an invalid identifier.
3. **Given** the barcode reader does not respond within the allowed time, **When** a read is requested, **Then** the system ends the request and reports a timeout failure.

---

### User Story 2 - Extend carrier identification to Omron and Hermes RFID protocols (Priority: P1)

As a deployment engineer, I need the controller to extend the same carrier-read contract to Omron ASCII RFID, Omron HEX RFID, and Hermes RFID readers so that existing site hardware can be reused without changing the caller interface.

**Why this priority**: Multi-device compatibility is required to install the same controller in environments that use different carrier identification hardware. US1 delivers the barcode path; this story extends coverage to all RFID variants.

**Independent Test**: For Omron ASCII, Omron HEX, and Hermes RFID, execute the same carrier-read workflow and confirm that the controller behavior is consistent from the caller perspective even though the device protocol differs.

**Acceptance Scenarios**:

1. **Given** the system is configured for an Omron ASCII RFID reader, **When** a carrier read is requested, **Then** the controller completes the Omron ASCII workflow and returns the result in the standard carrier-read format.
2. **Given** the system is configured for an Omron HEX RFID reader, **When** a carrier read is requested, **Then** the controller completes the Omron HEX workflow and returns the result in the standard carrier-read format.
3. **Given** the system is configured for a Hermes RFID reader, **When** a carrier read is requested, **Then** the controller completes the Hermes-specific workflow and returns the result in the standard carrier-read format.
4. **Given** any supported RFID reader does not respond within the allowed time, **When** a read is requested, **Then** the system ends the request and reports a timeout failure.

---

### User Story 3 - Write carrier data to supported RFID devices (Priority: P2)

As a service or integration workflow, I need to write carrier data back to supported RFID devices when required so that the equipment can update carrier information in a controlled and verifiable way.

**Why this priority**: Read support is the primary requirement, but controlled write capability is needed for sites that store or refresh carrier identity on RFID media.

**Independent Test**: For each RFID reader type that supports write operations, request a write using valid carrier data and confirm that the device acknowledges the update and the controller reports success.

**Acceptance Scenarios**:

1. **Given** a supported RFID device is available and writable media is present, **When** valid carrier data is submitted for writing, **Then** the system writes the data and reports success.
2. **Given** a supported RFID device rejects the write request, **When** valid carrier data is submitted, **Then** the system reports the write failure without claiming success.
3. **Given** a write request payload violates the configured reader type's supported page, encoding, length, or format constraints, **When** the write is requested, **Then** the system rejects the request with a deterministic validation failure before sending any device command.

---

### User Story 4 - Prevent invalid concurrent operations (Priority: P2)

As an equipment controller, I need the reader workflow to reject overlapping commands so that device state remains predictable and carrier data is not corrupted by concurrent operations.

**Why this priority**: Reader hardware protocols are stateful. Overlapping commands create ambiguous outcomes and increase the chance of communication or data errors.

**Independent Test**: Start one reader operation and attempt to start a second operation before the first completes; verify that the system rejects the second request with a clear failure result.

**Acceptance Scenarios**:

1. **Given** a reader operation is already in progress, **When** another read or write request is submitted to the same reader, **Then** the system rejects the new request and reports that the reader is busy.
2. **Given** the active operation completes or fails, **When** a new request is submitted afterward, **Then** the system accepts the new request normally.

### Edge Cases

- A device connection drops after the request starts but before a response is complete.
- A device returns malformed or incomplete carrier data.
- A barcode reader repeatedly reports unreadable media for the same carrier.
- An RFID device acknowledges a request but returns data that fails integrity checks.
- A write request is issued with carrier data that exceeds the supported device payload size.
- The configured reader type does not match the physical device connected on the port.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST support carrier identification through four reader types: barcode reader, Omron ASCII RFID reader, Omron HEX RFID reader, and Hermes RFID reader.
- **FR-002**: The system MUST expose a consistent carrier-read interaction to its callers regardless of which supported reader type is configured.
- **FR-003**: The system MUST execute the protocol-specific read sequence required by the configured reader type before returning a carrier identifier.
- **FR-004**: The system MUST return a success result only when the configured reader type completes its read sequence and produces a valid carrier identifier (non-null, non-empty ASCII string).
- **FR-005**: The system MUST report a failure result when a device does not respond, returns unreadable data, returns invalid data, or rejects the requested operation.
- **FR-006**: The barcode reader workflow MUST perform up to 8 read attempts per request, following the legacy BL600 flow, and MUST stop retrying once a valid identifier is read or the attempt limit is reached.
- **FR-007**: The barcode reader workflow MUST leave the reader in a safe end state after each request, regardless of whether the request succeeds or fails. The safe end state is defined as: trigger off (stop scanning) and disconnect the communication session.
- **FR-008**: The Omron ASCII RFID workflow MUST read carrier data using the ASCII-oriented device behavior defined by the reference flow.
- **FR-009**: The Omron HEX RFID workflow MUST read carrier data using the HEX-oriented device behavior defined by the reference flow.
- **FR-010**: The Hermes RFID workflow MUST validate response integrity before accepting returned carrier data.
- **FR-011**: The system MUST support writing carrier data to Omron ASCII RFID, Omron HEX RFID, and Hermes RFID reader types. The barcode reader is read-only and does not require write support.
- **FR-012**: The system MUST reject reader operations that are submitted while a previous operation for the same device is still in progress.
- **FR-013**: The system MUST enforce a default 10-second response time limit for device operations and return a timeout failure when that limit is exceeded.
- **FR-014**: The system MUST record sufficient operational diagnostics for successful operations and failures so that device communication problems can be investigated.
- **FR-015**: The system MUST parse device responses into an ASCII string carrier identifier before returning the result. For readers that return HEX-encoded data (e.g., Omron HEX), the reader implementation MUST convert the raw bytes to their ASCII string representation internally.
- **FR-016**: The system MUST validate the write payload against the configured RFID reader type before sending any write command.
- **FR-017**: The system MUST reject an RFID write request with a deterministic validation failure when the payload violates the configured reader type's supported encoding, length, or format constraints.

### Key Entities *(include if feature involves data)*

- **Carrier Identifier Request**: A request to read or write carrier identity information using the configured reader type.
- **Carrier Identifier Result**: The outcome of a reader request, including success or failure status, the resolved carrier identifier as an ASCII string when available, and the failure reason when unsuccessful.
- **Reader Configuration**: The configured device type and communication settings that determine which protocol flow the controller uses.
- **Reader Operation State**: The current execution state for a device request, including idle, in progress, completed, timed out, or failed.

## Clarifications

### Session 2026-03-10

- Q: 設備操作的預設逾時值應為多少？ → A: 10 秒 (保守值，容許較慢的 RFID 裝置)
- Q: 哪些讀取器類型需要支援寫入操作？ → A: Omron ASCII、Omron HEX、Hermes RFID（全部 RFID 類型）
- Q: Barcode reader 讀取失敗時的最大重試次數應為多少？ → A: 8 次，並依 lp204.cc 的 BL600 流程在讀取完成前持續嘗試，直到成功或達到上限。
- Q: 載體 ID 返回給呼叫者時應統一為何種格式？ → A: ASCII 字串 (string)，HEX 資料在讀取器內部轉為 ASCII 表示
- Q: Barcode reader 的「安全結束狀態」具體指什麼？ → A: 關閉觸發 (trigger off) 並斷開連線

### Session 2026-03-11

- Q: 是否允許為 CarrierIDReader 功能新增獨立測試檔？ → A: 允許，於 `AutoTest/TDKController.Tests/Unit/` 新增 5 個測試檔（1 個基底類別 + 4 個具體讀取器），並將此批准記錄於規格與計畫文件。
- Q: 若 `ICarrierIDReader` 現有成員不足，是否允許修改？ → A: 允許，但僅限使用者批准前提下補足本功能必要成員，且必須在計畫與任務文件中記錄這項限制。
- Q: 在本功能範圍內，是否批准將上述 `ICarrierIDReader` 修改視為本次 feature 的例外授權？ → A: 批准。核准來源為 2026-03-11 使用者批准，目標介面為 `ICarrierIDReader`；雖然參考介面預設應保持穩定，但本功能允許在最小必要範圍內修改 `ICarrierIDReader`，前提是變更僅限 Carrier ID Reader 功能直接需要的成員，且必須明確記錄於規格、計畫與任務文件。此例外不得擴及其他 feature 或其他介面，並明確排除 `IConnector` 與 `ExceptionManagement.HRESULT`，兩者仍維持不可修改。
- Q: RFID 寫入 payload 限制暫未定值時應如何規範？ → A: 先定義 validation requirement；具體限制值與裝置格式細節後續依 legacy logic 與 PlantUML 補齊。
- Q: 10 秒逾時限制應如何套用於重試與協定階段？ → A: 10 秒為單次裝置等待階段的預設逾時值；Barcode reader 的每次重試皆可各自使用一次 10 秒逾時，不與前一次重試共享預算；各協定可使用較短的內部等待階段，但單一等待階段不得超過 10 秒。
- Q: 在具體裝置限制值尚未完全回填前，RFID 寫入 payload validation 的最小落地規則為何？ → A: 採最小可實作規則：Omron ASCII 僅接受頁碼 1–30、長度固定 16 字元、且內容必須為可列印 ASCII 並排除控制字元；Omron HEX 僅接受頁碼 1–30、長度固定 16 字元、且內容必須為十六進位字元；Hermes RFID 僅接受頁碼 1–17、長度固定 16 字元、且內容必須為十六進位字元。凡不符合上述規則者，皆視為 invalid payload，必須在送出裝置命令前回傳 deterministic validation failure。

## Assumptions

- The supported reader types and their expected behaviors are defined by the provided PlantUML diagrams and legacy reference logic.
- Existing controller infrastructure already provides the communication path and logging facilities needed for reader operations.
- Caller-facing behavior should remain consistent even when the internal reader protocol differs.
- Only the four reader types named in the request are in scope for this feature.
- The bilingual quickstart requirement is satisfied by a single `quickstart.md` file that contains both zh-TW and en-US sections.
- Until protocol-specific limits are fully confirmed from legacy references, RFID payload validation will enforce the currently approved minimum implementation rules for reader-specific page ranges, length, encoding, and format, and later refinements may narrow or extend device-specific constraints based on validated legacy behavior.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For each of the four supported reader types, 100% of valid reference test cases return a carrier identifier and a success result.
- **SC-002**: For each supported reader type, 100% of timeout, unreadable-media, and malformed-response reference test cases return a failure result without returning a false carrier identifier.
- **SC-003**: 100% of overlapping-operation test cases are rejected while an active request is in progress for the same device.
- **SC-004**: 100% of supported RFID write test cases report success only after the device confirms the write operation.
- **SC-005**: Diagnostic logs are produced for 100% of failed reader operations with enough information to determine whether the failure was caused by timeout, communication loss, invalid response, or device rejection.