# Tasks: Carrier ID Reader Implementation

**Input**: Design documents from `/specs/002-carrier-id-reader-impl/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Included — plan.md explicitly defines test file structure and success criteria require test case validation.

**Organization**: Tasks are grouped by user story. US1 + US2 (both P1) are combined into one phase since they are interdependent (reading requires protocol implementations).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Verify project structure and test infrastructure

- [ ] T001 Create unit test folder at AutoTest/TDKController.Tests/Unit/ and verify TDKController.Tests.csproj references NUnit 3.x and Moq

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared enumerations, interface, configuration, and abstract base class that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T002 [P] Add CarrierIDReaderType enum (BarcodeReader=0, OmronASCII=1, OmronHex=2, HermesRFID=3) to TDKController/Interface/ICarrierIDReader.cs
- [ ] T003 [P] Add CarrierID error codes (-300 to -313) to TDKController/Interface/ErrorCode.cs per data-model.md error code table
- [ ] T004 [P] Implement ICarrierIDReader interface (CarrierIDReaderConfigPath, CarrierIDReaderType, ParseCarrierIDReaderData, GetCarrierID, SetCarrierID) in TDKController/Interface/ICarrierIDReader.cs per ICarrierIDReader-contract.md
- [ ] T005 [P] Implement CarrierIDReaderConfig class (ReaderType, TimeoutMs=10000, MaxRetryCount=3, Page) in TDKController/Config/CarrierIDReaderConfig.cs per data-model.md
- [ ] T006 Implement CarrierIDReader abstract base class in TDKController/Module/CarrierIDReader.cs — constructor injection (IConnector, ILogUtility, CarrierIDReaderConfig), _ackSignal (ManualResetEventSlim), _isBusy volatile flag, Connector property with event subscribe/unsubscribe, OnDataReceived virtual, SendCommand, WaitForResponse, CheckBusy, ReleaseBusy, SetCarrierID default (return CarrierIdError) per data-model.md and research.md R-05/R-07

**Checkpoint**: Foundation ready — interface, config, enums, and base class are compilable. User story implementation can begin.

---

## Phase 3: US1 + US2 — Read Carrier ID from All Protocols (Priority: P1) 🎯 MVP

**Goal**: Implement the full read workflow for all 4 reader types so that GetCarrierID returns a valid ASCII carrier identifier from any configured device.

**Independent Test**: Configure each reader type one at a time, mock IConnector to simulate device responses, call GetCarrierID, and confirm correct carrier identifier and ErrorCode.Success.

### Tests for US1 + US2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T007 [P] [US1] Write CarrierIDReader base class unit tests (constructor null checks, CheckBusy/ReleaseBusy behavior, Connector property event subscription, WaitForResponse timeout, SetCarrierID default returns CarrierIdError) in AutoTest/TDKController.Tests/Unit/CarrierIDReaderBaseTests.cs
- [ ] T008 [P] [US1] Write IDReaderBarcodeReader unit tests (GetCarrierID success with mocked ACK+barcode response, retry on NG up to 3 times, MotorON/MotorOFF ACK handling, timeout scenario, safe end state after failure) in AutoTest/TDKController.Tests/Unit/IDReaderBarcodeReaderTests.cs
- [ ] T009 [P] [US2] Write IDReaderOmronASCII read unit tests (GetCarrierID success with mocked completion code "00" + ASCII data, command failure with non-"00" code, timeout scenario, page bitmask validation) in AutoTest/TDKController.Tests/Unit/IDReaderOmronASCIITests.cs
- [ ] T010 [P] [US2] Write IDReaderOmronHex read unit tests (GetCarrierID success with mocked completion code "00" + HEX data, hex-to-ASCII conversion, command failure, timeout scenario) in AutoTest/TDKController.Tests/Unit/IDReaderOmronHexTests.cs
- [ ] T011 [P] [US2] Write IDReaderHermesRFID read unit tests (GetCarrierID success with mocked 'x' response + data, checksum validation pass/fail per R-09, error 'e' response handling, timeout scenario) in AutoTest/TDKController.Tests/Unit/IDReaderHermesRFIDTests.cs

### Implementation for US1 + US2

- [ ] T012 [P] [US1] Implement IDReaderBarcodeReader in TDKController/Module/IDReaderBarcodeReader.cs — CarrierIDReaderType override, OnDataReceived (parse ACK "OK" vs barcode data vs "NG"), MotorON/MotorOFF commands, ReadBarCode, GetCarrierID full flow (Connect → MotorON → Loop ReadBarCode max 3 retries → MotorOFF → Disconnect), ParseCarrierIDReaderData per research.md R-02
- [ ] T013 [P] [US2] Implement IDReaderOmronASCII in TDKController/Module/IDReaderOmronASCII.cs — CarrierIDReaderType override, OnDataReceived (parse completion code + data), BuildReadCommand with page bitmask per R-08, GetCarrierID (build command → send → wait → parse ASCII response), ParseCarrierIDReaderData per research.md R-03
- [ ] T014 [P] [US2] Implement IDReaderOmronHex in TDKController/Module/IDReaderOmronHex.cs — CarrierIDReaderType override, OnDataReceived (parse completion code + hex data), BuildReadCommand with page bitmask per R-08, GetCarrierID (build command → send → wait → parse hex → convert to ASCII string), ParseCarrierIDReaderData per research.md R-03
- [ ] T015 [P] [US2] Implement IDReaderHermesRFID in TDKController/Module/IDReaderHermesRFID.cs — CarrierIDReaderType override, OnDataReceived (command char decode 'x'/'e'/'n' + dual checksum verification per R-09), PrepareCommand (frame: 'S' + len + message + 0x0D + XOR checksum + ADD checksum), GetCarrierID (build read command "X0"+page → send → wait → verify checksum → parse data), ParseCarrierIDReaderData per research.md R-04

**Checkpoint**: All 4 reader types can read carrier IDs. GetCarrierID works for BarcodeReader, OmronASCII, OmronHex, and HermesRFID with mocked IConnector. MVP is functionally complete.

---

## Phase 4: US3 — Write Carrier Data to RFID Devices (Priority: P2)

**Goal**: Enable SetCarrierID on the 3 RFID reader types (Omron ASCII, Omron HEX, Hermes) so that carrier data can be written back to RFID media.

**Independent Test**: For each RFID reader type, mock IConnector to simulate write acknowledgment, call SetCarrierID with valid data, and confirm ErrorCode.Success. Also verify BarcodeReader.SetCarrierID returns CarrierIdError.

### Tests for US3

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T016 [P] [US3] Add OmronASCII write unit tests (SetCarrierID success with mocked "00" ack, invalid data length returns CarrierIdInvalidParameter, command failure, timeout) to AutoTest/TDKController.Tests/Unit/IDReaderOmronASCIITests.cs
- [ ] T017 [P] [US3] Add OmronHex write unit tests (SetCarrierID success with mocked "00" ack, invalid data length returns CarrierIdInvalidParameter, command failure, timeout) to AutoTest/TDKController.Tests/Unit/IDReaderOmronHexTests.cs
- [ ] T018 [P] [US3] Add HermesRFID write unit tests (SetCarrierID success with mocked 'w' ack, error 'e' response, invalid data length, timeout) to AutoTest/TDKController.Tests/Unit/IDReaderHermesRFIDTests.cs

### Implementation for US3

- [ ] T019 [P] [US3] Implement SetCarrierID override in TDKController/Module/IDReaderOmronASCII.cs — BuildWriteCommand (command type "02" + ASCII format + page bitmask + 16-byte data + 0x0D), CheckBusy → send → wait → parse ack → ReleaseBusy per research.md R-03
- [ ] T020 [P] [US3] Implement SetCarrierID override in TDKController/Module/IDReaderOmronHex.cs — BuildWriteCommand (command type "02" + HEX format + page bitmask + 16-byte data + 0x0D), CheckBusy → send → wait → parse ack → ReleaseBusy per research.md R-03
- [ ] T021 [P] [US3] Implement SetCarrierID override in TDKController/Module/IDReaderHermesRFID.cs — PrepareCommand with write message "W0"+page+16-char data, CheckBusy → send → wait → verify checksum → parse 'w' ack → ReleaseBusy per research.md R-04

**Checkpoint**: All RFID reader types support write operations. BarcodeReader correctly rejects write with CarrierIdError.

---

## Phase 5: US4 — Prevent Invalid Concurrent Operations (Priority: P2)

**Goal**: Validate that overlapping read/write requests to the same reader are rejected with CarrierIdBusy (-304) and that the reader accepts new requests after the previous one completes or fails.

**Independent Test**: Start a read operation (mock slow device response), attempt a second operation before the first completes, verify CarrierIdBusy is returned. Then complete the first operation and verify a new request succeeds.

### Tests for US4

- [ ] T022 [P] [US4] Add concurrency prevention tests (concurrent GetCarrierID returns CarrierIdBusy, concurrent SetCarrierID returns CarrierIdBusy, busy flag released after success, busy flag released after timeout, busy flag released after failure) to AutoTest/TDKController.Tests/Unit/CarrierIDReaderBaseTests.cs
- [ ] T023 [P] [US4] Add concurrency integration tests for at least one subclass (e.g., IDReaderBarcodeReader: start slow read with mocked delayed response, second GetCarrierID immediately returns CarrierIdBusy) to AutoTest/TDKController.Tests/Unit/IDReaderBarcodeReaderTests.cs

**Checkpoint**: Concurrency protection is validated across base class and subclass scenarios.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, edge case coverage, and documentation

- [ ] T024 [P] Add edge case tests (connection drop mid-operation, malformed response data, response exceeding buffer, Hermes checksum mismatch, write data exceeding payload size) across relevant test files in AutoTest/TDKController.Tests/Unit/
- [ ] T025 Verify XML documentation comments on all public API members across TDKController/Interface/ICarrierIDReader.cs, TDKController/Config/CarrierIDReaderConfig.cs, TDKController/Module/CarrierIDReader.cs, and all 4 subclass files
- [ ] T026 Build solution (msbuild TDKServer.sln) and run all unit tests (dotnet test AutoTest/TDKController.Tests/) to confirm zero errors
- [ ] T027 Run quickstart.md validation — execute usage examples from specs/002-carrier-id-reader-impl/quickstart.md in a test harness to confirm API surface matches documentation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — **BLOCKS all user stories**
- **US1 + US2 (Phase 3)**: Depends on Phase 2 completion — all 4 reader implementations are parallelizable
- **US3 (Phase 4)**: Depends on Phase 3 (adds write to existing reader files from Phase 3)
- **US4 (Phase 5)**: Depends on Phase 2 (base class logic) — can run in parallel with Phase 3/4 for test writing
- **Polish (Phase 6)**: Depends on Phases 3, 4, and 5

### User Story Dependencies

- **US1 + US2 (P1)**: Can start after Phase 2 — No dependencies on other stories
- **US3 (P2)**: Depends on US1+US2 (adds SetCarrierID to files created in Phase 3)
- **US4 (P2)**: Implementation is in Phase 2 base class; test validation can run after Phase 2

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Base class / interface must exist before subclass implementation
- All 4 subclass implementations within a phase can run in parallel ([P])
- Story complete before moving to next priority

### Parallel Opportunities

- Phase 2: T002, T003, T004, T005 can all run in parallel (different files)
- Phase 3 Tests: T007–T011 can all run in parallel (5 independent test files)
- Phase 3 Implementation: T012–T015 can all run in parallel (4 independent .cs files)
- Phase 4 Tests: T016–T018 can all run in parallel (3 independent test additions)
- Phase 4 Implementation: T019–T021 can all run in parallel (3 independent file edits)
- Phase 5: T022, T023 can run in parallel (different test files)

---

## Parallel Example: Phase 3 (US1 + US2)

```text
# Batch 1 — Launch all tests together (they will FAIL):
T007: CarrierIDReaderBaseTests.cs
T008: IDReaderBarcodeReaderTests.cs
T009: IDReaderOmronASCIITests.cs
T010: IDReaderOmronHexTests.cs
T011: IDReaderHermesRFIDTests.cs

# Batch 2 — Launch all implementations together:
T012: IDReaderBarcodeReader.cs
T013: IDReaderOmronASCII.cs (read)
T014: IDReaderOmronHex.cs (read)
T015: IDReaderHermesRFID.cs (read)

# All tests should now PASS
```

---

## Implementation Strategy

### MVP First (US1 + US2 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: US1 + US2 (all 4 reader types read)
4. **STOP and VALIDATE**: Build + run all unit tests
5. MVP is deployable — carrier ID reading works for all 4 device types

### Incremental Delivery

1. Setup + Foundational → Infrastructure ready
2. US1 + US2 → Read works for all devices → **MVP** ✅
3. US3 → Write works for RFID devices → Enhanced capability
4. US4 validation → Concurrency protection verified → Production-ready
5. Polish → Edge cases, docs, final validation → Release-quality

### File Summary

| File | Phase | Action |
|------|-------|--------|
| TDKController/Interface/ErrorCode.cs | 2 | Edit (add -300 series) |
| TDKController/Interface/ICarrierIDReader.cs | 2 | Edit (fill enum + interface) |
| TDKController/Config/CarrierIDReaderConfig.cs | 2 | Edit (fill class) |
| TDKController/Module/CarrierIDReader.cs | 2 | Edit (fill abstract base) |
| TDKController/Module/IDReaderBarcodeReader.cs | 3 | New |
| TDKController/Module/IDReaderOmronASCII.cs | 3+4 | New (read), Edit (write) |
| TDKController/Module/IDReaderOmronHex.cs | 3+4 | New (read), Edit (write) |
| TDKController/Module/IDReaderHermesRFID.cs | 3+4 | New (read), Edit (write) |
| AutoTest/TDKController.Tests/Unit/CarrierIDReaderBaseTests.cs | 3+5 | New |
| AutoTest/TDKController.Tests/Unit/IDReaderBarcodeReaderTests.cs | 3+5 | New |
| AutoTest/TDKController.Tests/Unit/IDReaderOmronASCIITests.cs | 3+4 | New |
| AutoTest/TDKController.Tests/Unit/IDReaderOmronHexTests.cs | 3+4 | New |
| AutoTest/TDKController.Tests/Unit/IDReaderHermesRFIDTests.cs | 3+4 | New |

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks in same batch
- [US1]/[US2]/[US3]/[US4] labels map tasks to spec.md user stories
- US1 + US2 are combined into Phase 3 because both are P1 and interdependent (reading requires protocol implementations)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- All reader subclass files follow the same constructor signature: (CarrierIDReaderConfig, IConnector, ILogUtility)
- Reference research.md for protocol details: R-02 (Barcode), R-03 (Omron), R-04 (Hermes), R-05 (signals), R-08 (bitmask), R-09 (checksum)
