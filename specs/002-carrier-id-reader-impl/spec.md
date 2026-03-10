# Feature Specification: Carrier ID Reader Implementation

**Feature Branch**: `002-carrier-id-reader-impl`  
**Created**: 2026-03-10  
**Status**: Draft  
**Input**: User description: "我要實作 TDKController CarrierIDReader中的檔案，請參考 CarrierIDReader.plantuml 來實作，其中 class IDReaderBarcodeReader, IDReaderOmronASCII, IDReaderOmronHex, IDReaderHermesRFID 分別建置一個檔案來實作。IDReaderBarcodeReader 請參考 BarcodeReader-Read.puml、BarcodeReader.puml、lp204.cc:7664-8043。IDReaderOmronASCII 請參考 OmronASCII.puml、OmronASCII-Read、lp204.cc:8695-9194 中 ASCII 的流程。IDReaderOmronHex 請參考 OmronHex.puml、OmronHex-Read.puml、lp204.cc:8695-9194 中 HEX 的流程。IDReaderHermesRFID 請參考 HermesRFID-Read.puml、HermesRFID.puml、lp204.cc:8048-8772。"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Read Carrier ID from any supported device (Priority: P1)

As an equipment controller, I need to read a carrier identifier from the configured device type so that the system can identify the carrier before the next handling step begins.

**Why this priority**: Carrier identification is a prerequisite for downstream routing, validation, and traceability. If the read fails, the equipment cannot continue safely.

**Independent Test**: Configure each supported reader type one at a time, trigger a read request with valid carrier media present, and confirm that the controller returns a usable carrier identifier and a success result.

**Acceptance Scenarios**:

1. **Given** the controller is configured for a supported reader type and valid carrier media is present, **When** a read is requested, **Then** the system returns the carrier identifier and reports success.
2. **Given** the controller is configured for a supported reader type but the carrier media cannot be read, **When** a read is requested, **Then** the system reports a read failure without returning an invalid identifier.
3. **Given** the configured reader does not respond within the allowed time, **When** a read is requested, **Then** the system ends the request and reports a timeout failure.

---

### User Story 2 - Support the full set of reader protocols already used in the field (Priority: P1)

As a deployment engineer, I need the controller to support barcode, Omron ASCII RFID, Omron HEX RFID, and Hermes RFID readers so that existing site hardware can be reused without changing the controller contract.

**Why this priority**: Multi-device compatibility is required to install the same controller in environments that use different carrier identification hardware.

**Independent Test**: For each supported reader type, execute the same carrier-read workflow and confirm that the controller behavior is consistent from the caller perspective even though the device protocol differs.

**Acceptance Scenarios**:

1. **Given** the system is configured for a barcode reader, **When** a carrier read is requested, **Then** the controller completes the barcode-specific workflow and returns the result in the standard carrier-read format.
2. **Given** the system is configured for an Omron ASCII or Omron HEX RFID reader, **When** a carrier read is requested, **Then** the controller completes the reader-specific workflow and returns the result in the standard carrier-read format.
3. **Given** the system is configured for a Hermes RFID reader, **When** a carrier read is requested, **Then** the controller completes the Hermes-specific workflow and returns the result in the standard carrier-read format.

---

### User Story 3 - Write carrier data to supported RFID devices (Priority: P2)

As a service or integration workflow, I need to write carrier data back to supported RFID devices when required so that the equipment can update carrier information in a controlled and verifiable way.

**Why this priority**: Read support is the primary requirement, but controlled write capability is needed for sites that store or refresh carrier identity on RFID media.

**Independent Test**: For each RFID reader type that supports write operations, request a write using valid carrier data and confirm that the device acknowledges the update and the controller reports success.

**Acceptance Scenarios**:

1. **Given** a supported RFID device is available and writable media is present, **When** valid carrier data is submitted for writing, **Then** the system writes the data and reports success.
2. **Given** a supported RFID device rejects the write request, **When** valid carrier data is submitted, **Then** the system reports the write failure without claiming success.

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
- **FR-004**: The system MUST return a success result only when the configured reader type completes its read sequence and produces a valid carrier identifier.
- **FR-005**: The system MUST report a failure result when a device does not respond, returns unreadable data, returns invalid data, or rejects the requested operation.
- **FR-006**: The barcode reader workflow MUST retry read attempts when the device reports an unreadable result, up to the device behavior defined by the existing reference flow, and MUST stop retrying once a valid identifier is read or the retry limit is reached.
- **FR-007**: The barcode reader workflow MUST leave the reader in a safe end state after each request, regardless of whether the request succeeds or fails.
- **FR-008**: The Omron ASCII RFID workflow MUST read carrier data using the ASCII-oriented device behavior defined by the reference flow.
- **FR-009**: The Omron HEX RFID workflow MUST read carrier data using the HEX-oriented device behavior defined by the reference flow.
- **FR-010**: The Hermes RFID workflow MUST validate response integrity before accepting returned carrier data.
- **FR-011**: The system MUST support writing carrier data to reader types whose reference behavior includes write support.
- **FR-012**: The system MUST reject reader operations that are submitted while a previous operation for the same device is still in progress.
- **FR-013**: The system MUST enforce finite response time limits for device operations and return a timeout failure when those limits are exceeded.
- **FR-014**: The system MUST record sufficient operational diagnostics for successful operations and failures so that device communication problems can be investigated.
- **FR-015**: The system MUST parse device responses into a caller-usable carrier identifier format before returning the result.

### Key Entities *(include if feature involves data)*

- **Carrier Identifier Request**: A request to read or write carrier identity information using the configured reader type.
- **Carrier Identifier Result**: The outcome of a reader request, including success or failure status, the resolved carrier identifier when available, and the failure reason when unsuccessful.
- **Reader Configuration**: The configured device type and communication settings that determine which protocol flow the controller uses.
- **Reader Operation State**: The current execution state for a device request, including idle, in progress, completed, timed out, or failed.

## Assumptions

- The supported reader types and their expected behaviors are defined by the provided PlantUML diagrams and legacy reference logic.
- Existing controller infrastructure already provides the communication path and logging facilities needed for reader operations.
- Caller-facing behavior should remain consistent even when the internal reader protocol differs.
- Only the four reader types named in the request are in scope for this feature.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For each of the four supported reader types, 100% of valid reference test cases return a carrier identifier and a success result.
- **SC-002**: For each supported reader type, 100% of timeout, unreadable-media, and malformed-response reference test cases return a failure result without returning a false carrier identifier.
- **SC-003**: 100% of overlapping-operation test cases are rejected while an active request is in progress for the same device.
- **SC-004**: 100% of supported RFID write test cases report success only after the device confirms the write operation.
- **SC-005**: Diagnostic logs are produced for 100% of failed reader operations with enough information to determine whether the failure was caused by timeout, communication loss, invalid response, or device rejection.