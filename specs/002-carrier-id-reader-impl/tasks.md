# Tasks: Carrier ID Reader Implementation

**Input**: Design documents from `/specs/002-carrier-id-reader-impl/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Included. The feature specification, contract, quickstart, and measurable success criteria require executable NUnit + Moq verification for each reader workflow.

**Organization**: Tasks are grouped by user story so each story can be implemented and validated independently while updating the existing 002 feature scope only.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on unfinished tasks)
- **[Story]**: Which user story this task belongs to (`[US1]`, `[US2]`, `[US3]`, `[US4]`)
- Every task includes an exact file or directory path

---

## Phase 1: Setup

**Purpose**: Reconfirm the existing Carrier ID Reader feature scaffold and test targets before updating implementation work.

- [X] T001 Verify the existing Carrier ID Reader source placeholders and approved file targets in TDKController/Interface/, TDKController/Config/, TDKController/Module/, and AutoTest/TDKController.Tests/Unit/
- [X] T002 Verify NUnit 3.x + Moq references and Unit test discovery configuration in AutoTest/TDKController.Tests/TDKController.Tests.csproj

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Update shared contracts, configuration defaults, timeout semantics, and base infrastructure required before any protocol-specific workflow can be completed.

**⚠️ CRITICAL**: No user story work should start before this phase is complete.

- [X] T003 [P] Record and apply the 2026-03-11 user-approved `ICarrierIDReader` exception scope for feature `002-carrier-id-reader-impl`, limited to the minimum members required for Carrier ID Reader read/write operations, and explicitly exclude any changes to `IConnector` and `ExceptionManagement.HRESULT`, in TDKController/Interface/ICarrierIDReader.cs and specs/002-carrier-id-reader-impl/contracts/ICarrierIDReader-contract.md
- [X] T004 [P] Add and verify Carrier ID Reader error codes for timeout, busy, validation, checksum, and retry-exhausted outcomes in TDKController/Interface/ErrorCode.cs
- [X] T005 [P] Update shared reader defaults for 10-second single-wait timeout semantics, Barcode max retry count 8, and reader-type page constraints in TDKController/Config/CarrierIDReaderConfig.cs
- [X] T006 Implement shared busy-guard, connector session, response wait, and cleanup infrastructure in TDKController/Module/CarrierIDReader.cs
- [X] T007 [P] Add base-class tests for busy guard, timeout propagation, and cleanup release paths in AutoTest/TDKController.Tests/Unit/CarrierIDReaderBaseTests.cs

**Checkpoint**: Shared contracts and base behavior compile cleanly and are ready for protocol-specific work.

---

## Phase 3: User Story 1 - Read Carrier ID from the configured barcode reader (Priority: P1) 🎯 MVP

**Goal**: Deliver the caller-facing read workflow for the Keyence BL600 barcode reader, including the legacy retry loop and safe end-state cleanup.

**Independent Test**: Configure BarcodeReader, simulate valid media, unreadable media, and timeout cases, and confirm GetCarrierID returns a valid ASCII identifier or the correct failure after up to 8 BL600 read attempts. Verify each attempt may consume its own 10-second device wait budget and that every request ends with trigger off and disconnected state.

### Tests for User Story 1

- [X] T008 [P] [US1] Add Barcode tests for success, unreadable media, retry exhaustion at 8 attempts, and stop-on-first-success behavior in AutoTest/TDKController.Tests/Unit/IDReaderBarcodeReaderTests.cs
- [X] T009 [P] [US1] Add Barcode timeout tests covering per-attempt 10-second wait semantics, non-shared retry budgets, and safe end-state cleanup in AutoTest/TDKController.Tests/Unit/IDReaderBarcodeReaderTests.cs

### Implementation for User Story 1

- [X] T010 [US1] Implement the BL600 connection, MotorON, LON or read, LOFF, MotorOFF, and disconnect sequence following the lp204.cc legacy flow in TDKController/Module/IDReaderBarcodeReader.cs
- [X] T011 [US1] Implement the Barcode retry loop with up to 8 read attempts, per-attempt timeout handling, NG or unreadable response handling, and stop-on-valid-read behavior in TDKController/Module/IDReaderBarcodeReader.cs
- [X] T012 [US1] Validate the Barcode workflow examples and expected failure behavior in specs/002-carrier-id-reader-impl/quickstart.md

**Checkpoint**: A caller can read a carrier identifier through the unified API using the Barcode reader path.

---

## Phase 4: User Story 2 - Extend carrier identification to Omron and Hermes RFID protocols (Priority: P1)

**Goal**: Extend the same GetCarrierID contract to Omron ASCII RFID, Omron HEX RFID, and Hermes RFID so deployed hardware variants behave consistently for callers.

**Independent Test**: For Omron ASCII, Omron HEX, and Hermes RFID, simulate valid device responses, malformed data, and timeout cases, then verify each implementation returns the carrier identifier through the same GetCarrierID API contract while enforcing per-wait-stage timeout limits no greater than 10 seconds.

### Tests for User Story 2

- [X] T013 [P] [US2] Add Omron ASCII read tests covering success, invalid page rejection, malformed payload handling, and timeout behavior in AutoTest/TDKController.Tests/Unit/IDReaderOmronASCIITests.cs
- [X] T014 [P] [US2] Add Omron HEX read tests covering success, HEX-to-ASCII conversion, malformed HEX payload handling, and timeout behavior in AutoTest/TDKController.Tests/Unit/IDReaderOmronHexTests.cs
- [X] T015 [P] [US2] Add Hermes RFID read tests covering success, checksum or integrity failures, invalid page rejection, and timeout behavior in AutoTest/TDKController.Tests/Unit/IDReaderHermesRFIDTests.cs

### Implementation for User Story 2

- [X] T016 [P] [US2] Implement the Omron ASCII read protocol, page-mask assembly, response parsing, and per-stage timeout handling in TDKController/Module/IDReaderOmronASCII.cs
- [X] T017 [P] [US2] Implement the Omron HEX read protocol, page-mask assembly, HEX-to-ASCII conversion, and per-stage timeout handling in TDKController/Module/IDReaderOmronHex.cs
- [X] T018 [P] [US2] Implement the Hermes RFID read protocol, checksum validation, response integrity checks, and per-stage timeout handling in TDKController/Module/IDReaderHermesRFID.cs

**Checkpoint**: All supported reader protocols expose consistent read behavior to the caller.

---

## Phase 5: User Story 3 - Write carrier data to supported RFID devices (Priority: P2)

**Goal**: Add SetCarrierID support for Omron ASCII, Omron HEX, and Hermes RFID while enforcing deterministic reader-type-specific payload validation before any device command is sent.

**Independent Test**: Submit valid carrier data to each supported RFID implementation, simulate positive acknowledgements, and confirm SetCarrierID reports success. Submit invalid page numbers, invalid encodings, invalid lengths, and rejected writes, then confirm the implementation returns deterministic validation or command-failure results without sending unsupported payloads.

### Tests for User Story 3

- [X] T019 [P] [US3] Add Omron ASCII write tests covering valid writes and deterministic validation failures for pages outside 1-30, non-printable ASCII, control characters, and non-16-character payloads in AutoTest/TDKController.Tests/Unit/IDReaderOmronASCIITests.cs
- [X] T020 [P] [US3] Add Omron HEX write tests covering valid writes and deterministic validation failures for pages outside 1-30, non-hex characters, and non-16-character payloads in AutoTest/TDKController.Tests/Unit/IDReaderOmronHexTests.cs
- [X] T021 [P] [US3] Add Hermes RFID write tests covering valid writes and deterministic validation failures for pages outside 1-17, non-hex characters, and non-16-character payloads in AutoTest/TDKController.Tests/Unit/IDReaderHermesRFIDTests.cs

### Implementation for User Story 3

- [X] T022 [P] [US3] Implement Omron ASCII write flow and pre-send payload validation for page range 1-30, fixed 16 printable ASCII characters, and control-character rejection in TDKController/Module/IDReaderOmronASCII.cs
- [X] T023 [P] [US3] Implement Omron HEX write flow and pre-send payload validation for page range 1-30, fixed 16 hexadecimal characters, and deterministic invalid-parameter failures in TDKController/Module/IDReaderOmronHex.cs
- [X] T024 [P] [US3] Implement Hermes RFID write flow and pre-send payload validation for page range 1-17, fixed 16 hexadecimal characters, and deterministic invalid-parameter failures in TDKController/Module/IDReaderHermesRFID.cs

**Checkpoint**: Supported RFID readers can write carrier data and reject invalid payloads before device I/O begins.

---

## Phase 6: User Story 4 - Prevent invalid concurrent operations (Priority: P2)

**Goal**: Ensure overlapping read and write requests on the same reader instance are rejected predictably and that busy state is released after completion, validation failure, failure, or timeout.

**Independent Test**: Start one operation, issue a second request before the first completes, verify CarrierIdBusy is returned without sending extra device commands, then complete, fail, or timeout the first request and confirm the next request is accepted normally.

### Tests for User Story 4

- [X] T025 [P] [US4] Add overlapping-operation and no-extra-send tests in AutoTest/TDKController.Tests/Unit/CarrierIDReaderBaseTests.cs
- [X] T026 [P] [US4] Add reader-specific concurrency regression tests for read, write, validation-failure, and timeout release paths in AutoTest/TDKController.Tests/Unit/IDReaderBarcodeReaderTests.cs and AutoTest/TDKController.Tests/Unit/IDReaderOmronASCIITests.cs

### Implementation for User Story 4

- [X] T027 [US4] Harden busy-state acquisition and release for success, validation failure, command failure, and timeout cleanup in TDKController/Module/CarrierIDReader.cs

**Checkpoint**: The reader base layer rejects concurrent operations and recovers cleanly after terminal outcomes.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, documentation alignment, and cross-story verification of the current 002 feature scope.

- [X] T028 [P] Add cross-story edge-case tests for connection drop mid-request, malformed or incomplete responses, repeated unreadable Barcode media, Hermes integrity failures, and configured reader type mismatch in AutoTest/TDKController.Tests/Unit/
- [X] T029 [P] Align quickstart examples and verification notes with the 8-attempt Barcode flow and 10-second single-wait timeout semantics in specs/002-carrier-id-reader-impl/quickstart.md
- [X] T030 [P] Verify XML documentation, English implementation comments, and protocol-to-spec traceability across TDKController/Interface/ and TDKController/Module/
- [X] T031 [P] Verify try-catch plus logging compliance and diagnostic failure logging for timeout, communication loss, invalid response, validation failure, and device rejection paths in TDKController/Module/
- [ ] T032 Run coverage measurement for Carrier ID Reader logic from AutoTest/TDKController.Tests/TDKController.Tests.csproj and confirm the required core coverage target
- [X] T033 Run the Carrier ID Reader unit test suite from AutoTest/TDKController.Tests/TDKController.Tests.csproj
- [X] T034 Validate the documented zh-TW and en-US read and write scenarios in specs/002-carrier-id-reader-impl/quickstart.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1: Setup** has no dependencies and can start immediately.
- **Phase 2: Foundational** depends on Phase 1 and blocks all user story work.
- **Phase 3: US1** depends on Phase 2.
- **Phase 4: US2** depends on Phase 2 and can start after the shared base is ready; it should follow US1 for MVP-first delivery.
- **Phase 5: US3** depends on Phase 4 because RFID write support extends the RFID reader files created there.
- **Phase 6: US4** depends on Phase 2 and should run after the base layer and at least one concrete reader workflow exist.
- **Phase 7: Polish** depends on the completion of the selected user stories.

### User Story Dependencies

- **US1**: Starts after Foundational and delivers the first end-to-end carrier read path.
- **US2**: Starts after Foundational and extends the caller contract to the remaining protocols.
- **US3**: Depends on US2 because it adds write behavior to the RFID reader implementations.
- **US4**: Depends on Foundational base behavior and validates it against implemented reader workflows.

### Within Each User Story

- Test tasks should be created before implementation tasks and should fail before the implementation is added.
- Shared contracts and configuration must be completed before protocol-specific reader files.
- Barcode read flow cleanup must be verified together with retry and timeout behavior.
- RFID payload validation must be implemented before any RFID write command is sent.
- Busy-state validation should be completed after the shared base and concrete reader flows exist.

### Parallel Opportunities

- T003, T004, T005, and T007 can run in parallel in Phase 2.
- T008 and T009 can run in parallel in Phase 3.
- T013, T014, and T015 can run in parallel in Phase 4.
- T016, T017, and T018 can run in parallel in Phase 4.
- T019, T020, and T021 can run in parallel in Phase 5.
- T022, T023, and T024 can run in parallel in Phase 5.
- T025 and T026 can run in parallel in Phase 6.
- T028, T029, T030, and T031 can run in parallel in Phase 7.

---

## Parallel Example: User Story 1

```text
# Parallel Barcode verification batch
T008 -> AutoTest/TDKController.Tests/Unit/IDReaderBarcodeReaderTests.cs
T009 -> AutoTest/TDKController.Tests/Unit/IDReaderBarcodeReaderTests.cs
```

## Parallel Example: User Story 2

```text
# Parallel test batch for remaining reader protocols
T013 -> AutoTest/TDKController.Tests/Unit/IDReaderOmronASCIITests.cs
T014 -> AutoTest/TDKController.Tests/Unit/IDReaderOmronHexTests.cs
T015 -> AutoTest/TDKController.Tests/Unit/IDReaderHermesRFIDTests.cs

# Parallel implementation batch for remaining reader protocols
T016 -> TDKController/Module/IDReaderOmronASCII.cs
T017 -> TDKController/Module/IDReaderOmronHex.cs
T018 -> TDKController/Module/IDReaderHermesRFID.cs
```

## Parallel Example: User Story 3

```text
# Parallel RFID write-validation test batch
T019 -> AutoTest/TDKController.Tests/Unit/IDReaderOmronASCIITests.cs
T020 -> AutoTest/TDKController.Tests/Unit/IDReaderOmronHexTests.cs
T021 -> AutoTest/TDKController.Tests/Unit/IDReaderHermesRFIDTests.cs

# Parallel RFID write-validation implementation batch
T022 -> TDKController/Module/IDReaderOmronASCII.cs
T023 -> TDKController/Module/IDReaderOmronHex.cs
T024 -> TDKController/Module/IDReaderHermesRFID.cs
```

## Parallel Example: User Story 4

```text
# Parallel concurrency verification batch
T025 -> AutoTest/TDKController.Tests/Unit/CarrierIDReaderBaseTests.cs
T026 -> AutoTest/TDKController.Tests/Unit/IDReaderBarcodeReaderTests.cs and AutoTest/TDKController.Tests/Unit/IDReaderOmronASCIITests.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: User Story 1.
4. Stop and validate the BL600 read workflow, including 8-attempt retry and safe end-state cleanup, before expanding protocol coverage.

### Incremental Delivery

1. Setup + Foundational establish the shared reader contract and timeout semantics.
2. US1 delivers the first shippable Barcode read path.
3. US2 extends read support to Omron ASCII, Omron HEX, and Hermes RFID without changing the caller contract.
4. US3 adds RFID write capability with deterministic payload validation.
5. US4 hardens concurrent-operation behavior.
6. Polish completes edge cases, documentation alignment, and verification.

### Suggested MVP Scope

US1 only. It delivers a usable end-to-end carrier read slice with the shared reader contract while isolating subsequent protocol expansion and RFID write work.

---

## Notes

- All tasks follow the required checklist format: checkbox, sequential Task ID, optional `[P]`, required `[US#]` for story phases, and explicit file or directory path.
- This task list updates the existing 002 feature only and does not create a new feature number or specs directory.
- The contract document maps directly to T003, T004, and T007.
- The latest spec clarifications map directly to Barcode retry and timeout tasks (T005, T008, T009, T010, T011) and RFID payload validation tasks (T019 through T024).
