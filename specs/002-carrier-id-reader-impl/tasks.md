# Tasks: Carrier ID Reader Implementation

**Input**: Design documents from `/specs/002-carrier-id-reader-impl/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Included. The design artifacts define NUnit/Moq coverage, unit test file targets, quickstart validation, and measurable success criteria that require executable verification.

**Organization**: Tasks are grouped by user story so each story can be implemented and validated incrementally.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on unfinished tasks)
- **[Story]**: Which user story this task belongs to (`[US1]`, `[US2]`, `[US3]`, `[US4]`)
- Every task includes an exact file or directory path

---

## Phase 1: Setup

**Purpose**: Confirm the project and test scaffold needed for Carrier ID Reader work.

- [ ] T001 Verify Carrier ID Reader source placeholders in TDKController/Interface/, TDKController/Config/, and TDKController/Module/
- [ ] T002 Verify NUnit 3.x + Moq references and Unit test folder usage in AutoTest/TDKController.Tests/TDKController.Tests.csproj

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared contracts, configuration, error codes, and base infrastructure required before any reader workflow can be implemented.

**⚠️ CRITICAL**: No user story work should start before this phase is complete.

- [ ] T003 [P] Verify ICarrierIDReader contract compatibility, add CarrierIDReaderType, and only add user-approved missing members in TDKController/Interface/ICarrierIDReader.cs
- [ ] T004 [P] Add Carrier ID Reader error codes (-300 to -313) in TDKController/Interface/ErrorCode.cs
- [ ] T005 [P] Implement reader configuration fields and defaults in TDKController/Config/CarrierIDReaderConfig.cs
- [ ] T006 Implement the abstract CarrierIDReader base infrastructure in TDKController/Module/CarrierIDReader.cs
- [ ] T007 [P] Add shared base-class verification tests in AutoTest/TDKController.Tests/Unit/CarrierIDReaderBaseTests.cs

**Checkpoint**: Shared contracts and base behavior compile cleanly and are ready for protocol-specific work.

---

## Phase 3: User Story 1 - Read Carrier ID from the configured barcode reader (Priority: P1) 🎯 MVP

**Goal**: Deliver the caller-facing GetCarrierID workflow end-to-end through the unified reader contract with the first production protocol implementation.

**Independent Test**: Configure BarcodeReader, simulate success, unreadable media, and timeout responses, then confirm GetCarrierID returns the expected ASCII identifier or failure result.

### Tests for User Story 1

- [ ] T008 [P] [US1] Add Barcode read workflow tests in AutoTest/TDKController.Tests/Unit/IDReaderBarcodeReaderTests.cs

### Implementation for User Story 1

- [ ] T009 [US1] Implement the Barcode BL600 read workflow in TDKController/Module/IDReaderBarcodeReader.cs
- [ ] T010 [US1] Validate Barcode read examples and expected outcomes in bilingual sections of specs/002-carrier-id-reader-impl/quickstart.md

**Checkpoint**: A caller can read a carrier identifier through the unified API using the Barcode reader path.

---

## Phase 4: User Story 2 - Extend carrier identification to Omron and Hermes RFID protocols (Priority: P1)

**Goal**: Extend the same GetCarrierID contract to Omron ASCII RFID, Omron HEX RFID, and Hermes RFID so deployed hardware variants behave consistently for callers.

**Independent Test**: For Omron ASCII, Omron HEX, and Hermes RFID, simulate valid device responses and verify each implementation returns the carrier identifier through the same GetCarrierID API contract.

### Tests for User Story 2

- [ ] T011 [P] [US2] Add Omron ASCII read tests in AutoTest/TDKController.Tests/Unit/IDReaderOmronASCIITests.cs
- [ ] T012 [P] [US2] Add Omron HEX read tests in AutoTest/TDKController.Tests/Unit/IDReaderOmronHexTests.cs
- [ ] T013 [P] [US2] Add Hermes RFID read tests in AutoTest/TDKController.Tests/Unit/IDReaderHermesRFIDTests.cs

### Implementation for User Story 2

- [ ] T014 [P] [US2] Implement the Omron ASCII read protocol in TDKController/Module/IDReaderOmronASCII.cs
- [ ] T015 [P] [US2] Implement the Omron HEX read protocol and HEX-to-ASCII conversion in TDKController/Module/IDReaderOmronHex.cs
- [ ] T016 [P] [US2] Implement the Hermes RFID read protocol and checksum validation in TDKController/Module/IDReaderHermesRFID.cs

**Checkpoint**: All supported reader protocols expose consistent read behavior to the caller.

---

## Phase 5: User Story 3 - Write carrier data to supported RFID devices (Priority: P2)

**Goal**: Add SetCarrierID support for Omron ASCII, Omron HEX, and Hermes RFID while preserving the shared interface contract and enforcing reader-type-specific payload validation rules.

**Independent Test**: Submit valid carrier data to each supported RFID implementation, simulate positive acknowledgements, and confirm SetCarrierID reports success; invalid payloads, validation failures, and rejected writes must report failure.

### Tests for User Story 3

- [ ] T017 [P] [US3] Add Omron ASCII write tests in AutoTest/TDKController.Tests/Unit/IDReaderOmronASCIITests.cs
- [ ] T018 [P] [US3] Add Omron HEX write tests in AutoTest/TDKController.Tests/Unit/IDReaderOmronHexTests.cs
- [ ] T019 [P] [US3] Add Hermes RFID write tests in AutoTest/TDKController.Tests/Unit/IDReaderHermesRFIDTests.cs

### Implementation for User Story 3

- [ ] T020 [P] [US3] Implement RFID write flow and payload validation in TDKController/Module/IDReaderOmronASCII.cs
- [ ] T021 [P] [US3] Implement RFID write flow and payload validation in TDKController/Module/IDReaderOmronHex.cs
- [ ] T022 [P] [US3] Implement RFID write flow and payload validation in TDKController/Module/IDReaderHermesRFID.cs

**Checkpoint**: Supported RFID readers can write carrier data and report success only after device acknowledgement.

---

## Phase 6: User Story 4 - Prevent invalid concurrent operations (Priority: P2)

**Goal**: Ensure overlapping read and write requests on the same reader instance are rejected predictably and that busy state is released after completion, failure, or timeout.

**Independent Test**: Start one operation, issue a second request before the first completes, verify CarrierIdBusy is returned, then complete or fail the first request and confirm the next request is accepted.

### Tests for User Story 4

- [ ] T023 [P] [US4] Add busy-guard and release-path tests in AutoTest/TDKController.Tests/Unit/CarrierIDReaderBaseTests.cs
- [ ] T024 [P] [US4] Add overlapping-operation regression tests in AutoTest/TDKController.Tests/Unit/IDReaderBarcodeReaderTests.cs

### Implementation for User Story 4

- [ ] T025 [US4] Harden busy-state release and failure cleanup behavior in TDKController/Module/CarrierIDReader.cs

**Checkpoint**: The reader base layer rejects concurrent operations and recovers cleanly after terminal outcomes.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, documentation alignment, compliance verification, and edge-case coverage across stories.

- [ ] T026 [P] Add edge-case tests covering all spec scenarios: (1) connection drop mid-request, (2) malformed/incomplete device response, (3) barcode repeated unreadable media, (4) RFID integrity check failure, (5) write payload exceeding device limit, (6) reader type mismatch with physical device — in AutoTest/TDKController.Tests/Unit/
- [ ] T027 [P] Verify XML documentation, English inline implementation comments, and protocol-to-spec traceability comments in TDKController/Interface/ and TDKController/Module/
- [ ] T028 [P] Verify try-catch + logging pattern compliance and failure-path logging context on all public/internal methods per constitution rules in TDKController/Module/
- [ ] T029 [P] Verify FR-014/SC-005: confirm diagnostic logs are produced for all failure paths (timeout, comm loss, invalid response, device rejection) in AutoTest/TDKController.Tests/Unit/
- [ ] T030 Run coverage measurement on Carrier ID Reader test suite and verify >= 90% core logic coverage from AutoTest/TDKController.Tests/TDKController.Tests.csproj
- [ ] T031 Build and run the full Carrier ID Reader test suite from AutoTest/TDKController.Tests/TDKController.Tests.csproj
- [ ] T032 Validate documented zh-TW and en-US usage and failure cases in specs/002-carrier-id-reader-impl/quickstart.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1: Setup** has no dependencies and can start immediately.
- **Phase 2: Foundational** depends on Phase 1 and blocks all user story work.
- **Phase 3: US1** depends on Phase 2.
- **Phase 4: US2** depends on Phase 2 and can start after the shared base is ready; in practice it should follow US1 for MVP-first delivery.
- **Phase 5: US3** depends on Phase 4 because RFID write support extends the RFID reader files created there.
- **Phase 6: US4** depends on Phase 2 and should run after Phase 3 begins so concurrency behavior is validated against real reader workflows.
- **Phase 7: Polish** depends on the completion of the selected user stories.

### User Story Dependencies

- **US1**: Starts after Foundational and delivers the first end-to-end carrier read path.
- **US2**: Starts after Foundational and extends US1's caller contract to the remaining field protocols.
- **US3**: Depends on US2 because it adds write behavior to the RFID reader implementations.
- **US4**: Depends on Foundational base behavior and validates it against the implemented reader workflows.

### Within Each User Story

- Test tasks should be created before implementation tasks and should fail before the implementation is added.
- Shared contracts and configuration must be completed before protocol-specific reader files.
- Read flows should be completed before RFID write flows in the same reader file.
- Busy-state validation should be completed after the shared base and at least one concrete reader flow exist.

### Parallel Opportunities

- T003, T004, T005, and T007 can run in parallel in Phase 2.
- T011, T012, and T013 can run in parallel in Phase 4.
- T014, T015, and T016 can run in parallel in Phase 4.
- T017, T018, and T019 can run in parallel in Phase 5.
- T020, T021, and T022 can run in parallel in Phase 5.
- T023 and T024 can run in parallel in Phase 6.
- T026, T027, T028, and T029 can run in parallel in Phase 7.

---

## Parallel Example: User Story 2

```text
# Parallel test batch for the remaining reader protocols
T011 -> AutoTest/TDKController.Tests/Unit/IDReaderOmronASCIITests.cs
T012 -> AutoTest/TDKController.Tests/Unit/IDReaderOmronHexTests.cs
T013 -> AutoTest/TDKController.Tests/Unit/IDReaderHermesRFIDTests.cs

# Parallel implementation batch for the remaining reader protocols
T014 -> TDKController/Module/IDReaderOmronASCII.cs
T015 -> TDKController/Module/IDReaderOmronHex.cs
T016 -> TDKController/Module/IDReaderHermesRFID.cs
```

## Parallel Example: User Story 3

```text
# Parallel write-support test batch
T017 -> AutoTest/TDKController.Tests/Unit/IDReaderOmronASCIITests.cs
T018 -> AutoTest/TDKController.Tests/Unit/IDReaderOmronHexTests.cs
T019 -> AutoTest/TDKController.Tests/Unit/IDReaderHermesRFIDTests.cs

# Parallel write-support implementation batch
T020 -> TDKController/Module/IDReaderOmronASCII.cs
T021 -> TDKController/Module/IDReaderOmronHex.cs
T022 -> TDKController/Module/IDReaderHermesRFID.cs
```

## Parallel Example: User Story 4

```text
# Parallel concurrency validation batch
T023 -> AutoTest/TDKController.Tests/Unit/CarrierIDReaderBaseTests.cs
T024 -> AutoTest/TDKController.Tests/Unit/IDReaderBarcodeReaderTests.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: User Story 1.
4. Stop and validate Barcode-based carrier read behavior before expanding protocol coverage.

### Incremental Delivery

1. Setup + Foundational establish the shared reader contract and infrastructure.
2. US1 delivers the first shippable carrier read path.
3. US2 extends read support to the remaining field protocols without changing the caller contract.
4. US3 adds RFID write capability.
5. US4 hardens concurrency behavior.
6. Polish completes edge cases, documentation, and verification.

### Suggested MVP Scope

US1 only. It delivers a usable end-to-end carrier read slice with the shared reader contract, while keeping subsequent protocol expansion and RFID write work isolated.

---

## Notes

- All tasks follow the required checklist format: checkbox, sequential Task ID, optional `[P]`, required `[US#]` for story phases, and explicit file or directory path.
- The interface contract in `contracts/ICarrierIDReader-contract.md` maps to foundational tasks T003 and T007.
- The data model drives T004, T005, and T006.
- Research decisions map directly to protocol tasks: Barcode (T009), Omron ASCII/HEX (T014, T015, T020, T021), Hermes (T016, T022), synchronization (T006, T025), checksum validation (T016, T026).
