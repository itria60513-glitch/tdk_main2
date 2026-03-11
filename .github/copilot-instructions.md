# Project Guidelines

# Copilot Chat 回覆語言規則

- **所有 Copilot Chat 回覆必須使用繁體中文**（除非使用者明確要求其他語言）。


## Code Style

- **Language**: C# (.NET Framework 4.7.2). All comments and XML docs in **English**; specs are in Traditional Chinese.
- **Naming**: PascalCase for classes/methods, camelCase for locals, `_camelCase` for private fields, `I`-prefix for interfaces.
- **Error codes**: Return `int` (not enum). Use `const int` fields. `0` = success, `1-99` = info, `100-199` = warning, negative = error (module-specific ranges: E84 `-1..-99`, LoadportActor `-100..-199`, N2Purge `-200..-299`, CarrierIDReader `-300..-399`, LightCurtain `-400..-499`).
- **Methods**: prefer <=50 lines, <=3 nesting levels, <=4 parameters. Public/internal methods must be wrapped in try-catch with logging before rethrow.
- **Constructor injection**: null-check with `ArgumentNullException`, store as `private readonly`.
- **Event dependencies**: use property with subscribe/unsubscribe pattern in setter (see `constitution_CH.md` Section 4.2).
- **Single `.cs` file per module**. No new classes or files without explicit user approval.
- Reference patterns: `TDKLogUtility/Module/LogUtilityClient.cs` (DI, logging, error handling).

## Architecture

```
Service Layer     (TDKService) - Host communication, command parsing, response formatting
    |
Controller Layer  (TDKController) - Facade exposing ILoadportController
    |
Module Layer      (LoadportActor, N2Purge, CarrierIDReader, LightCurtain, E84)
    |
Infrastructure    (TDKLogUtility [READ-ONLY], Communication, Config)
```

- Upper layers may inject and use lower layers. Lower layers must NOT reference upper layers.
- Same-layer modules interact through interfaces only.
- **Read-only**: `TDKLogUtility/` project, `IConnector` interface, `HRESULT` type. Do not modify.
- Key interfaces: `ILogUtility` at `TDKLogUtility/Interface/ILogUtility.cs`, `IConnector` at `Communication/Interface/IConnector.cs`.

## Build and Test

```bash
# Build solution
msbuild TDKServiceMiniPC.sln /p:Configuration=Debug

# Build single project
msbuild TDKLogUtility/TDKLogUtility.csproj /p:Configuration=Debug

# Tests (NUnit 3.x + Moq, test projects under AutoTest/)
nunit3-console.exe AutoTest/[ProjectName].Tests/bin/Debug/[ProjectName].Tests.dll
```

Test naming: `MethodName_Scenario_ExpectedResult`. Target >=80% coverage for core logic.

## Project Conventions

- **Governing document**: `constitution_CH.md` (v3.0.1) is authoritative. All development must comply.
- **Reference implementation**: `lp204.cc` (~10,260 lines C++) is the existing loadport controller being ported to C#. All hardware interaction patterns originate here.

### TDK A Protocol (TAS300 Hardware)

Frame format: `SOH(0x01) | LEN(2 bytes) | ADR | CMD | CSh | CSl | DEL(0x03/0x0D)`. RS-232C at 9600 bps, 8N1, no flow control. Checksum = sum bytes LEN..CMD, low 8 bits, encode as 2-char ASCII hex. Two-stage timeout: ACK (5s), then INF/ABS (10s per E191E372 §3.4). Omission mode enabled (no FIN). See `E191E372 Interface Specifications.md`.

### Command Handshaking

Two-tier semaphore pattern (`lp204.cc:1289-1301`):
- **Quick commands** (GET/EVT/MOD): Send -> `sem_wait(semInfCmdACK)` for ACK only.
- **Operation commands** (MOV/SET): Send -> `sem_wait(semOpCmdACK)` for ACK -> `sem_wait(semOpCmdINF)` for INF/ABS completion.
- Response codes: `RES_ACK(0)`, `RES_NAK(1)`, `RES_INF(2)`, `RES_ABS(3)`.
- See `LP_Method_Procedure.md` for all 44+ method flows.

### Host Command Protocol

Commands arrive as `io <command> [params]\r\n`, parsed token-by-token (`lp204.cc:5203-6404`). Responses: `io <command> <status> [data]\r\n`. Status codes: `0x1` = success, `0x2` = accepted/executing, `0xc017` = execution failed, `0xc021` = busy/not ready.

Supported commands: `init`, `initx 2`, `load [1|2]`, `unload [1|2]`, `evon <id>`, `evoff <id>`, `id`, `statfxl`, `statnzl`, `stat_m`, `stat_pdo`, `stat_lp`, `lamp <id> <act>`, `map`, `rmap`, `rdid <page>`, `wrid <page> <lotID>`, `resid`, `e84t <tp1-tp6> <td1>`, `smcr`, `ene84nz <0|1>`, `enltc <0|1>`, `getconf`, `setconf <p1-p4>`, `shutdown [0|1]`, `update <len>`, `ene84 <onoff> <addr>`, `rde84 <addr>`, `ho_avbl <val> <addr>`, `es <val> <addr>`, `out_e84 <hex4> <addr>`, `act_purge`, `deact_purge`, `date [YYYY MM DD hh mm]`, `ver_sbc`, `mrt <val>`, `esmode <mode>`, `mch <hex>`.

### State Machines

- **Program state** (TDKService, not Module): `prg_NOTINIT(0)` -> `prg_READY(1)` <-> `prg_BUSY(2)`. Managed by TDKService. Busy guard pattern rejects commands when busy.
- **Fixload/AMHS state** (LoadportActor, `lp204.cc:1078-1082`): `fxl_NOTINIT(0)`, `fxl_READY(1)`, `fxl_BUSY(2)`, `fxl_AMHS(3)`.
- **FOUP status** (LoadportActor, `lp204.cc:1197-1202`): `FPS_UNKNOWN(-1)`, `FPS_NOFOUP(0)`, `FPS_PLACED(1)`, `FPS_CLAMPED(2)`, `FPS_DOCKED(3)`, `FPS_OPENED(4)`.

### E84 AMHS State Machine

11-state cycle for automated load/unload (`lp204.cc:1452-1465, 1857-2501`):

`E84_ENABLED(0)` -> `E84_CS_ON(1)` -> `E84_VALID_ON(2)` -> `E84_LUREQ_ON(3)` -> `E84_TRREQ_ON(4)` -> `E84_READY_ON(5)` -> `E84_BUSY_ON(6)` -> `E84_LUREQ_OF(7)` -> `E84_COMPT_ON(10)` -> `E84_READY_OF(11)` -> `E84_VALID_OF(12)` -> back to ENABLED.

Timing constants (configurable via `e84t` command): TP1-TP6, TD1. Default: TP1=2s, TP2=2s, TP3=60s, TP4=60s, TP5=2s, TP6=2s, TD1=1s. 100ms polling interval.

Error handlers: `ErrSignalProcess`, `TimeOutProcess` (codes `0x3c50-0x3c57`), `LTCViolated`, `MchDoorOpened`, `AbortSequence`.

### Special Event Codes

Reported via `evon`/`evoff` to host (`lp204.cc:73-94`):
- `0x8000`: TAS300 error + startup event
- `0x8001`: TAS300 status
- `0x8002`: Operation return code
- `0x8003`/`0x8004`: N2 purge status/PGWFL
- `0x8010`: Program update error
- `0x8021`: Barcode errors (0x01=motor fail, 0x02=timeout, 0x03=NG, 0x04=ERROR, 0x05=finish flag)
- `0x8024`: Hermos RFID errors (0x0e, 0x10=timeout)
- `0x8027`: Omron RFID errors (0x1*=comm, 0x7*=hardware, 0xF0=timeout)
- `0x8030`: E84 I/O and FOUP status
- `0x8031`: E84 loop error code

## Integration Points

- **TAS300 loadport** (`CTas300`, `lp204.cc:1232-1306`): Serial RS-232C at 9600/8N1. TDK A framed protocol. Commands: `ORGSH`, `CLOAD`, `CULOD`, `PODCL`, `CLDYD`, `CULYD`, `CULFC`, `MAPDO`, `ABORG`, `BPNUP`/`BPNDW`. Status struct at `lp204.cc:1243-1263` has 18 sensor fields (eqpStatus, fpPlace, fpClamp, zPos, yPos, etc.).
- **Host controller** (`CHostBK`, `lp204.cc:1088-1108`): Serial RS-232C at 9600/8N1. Text protocol with `\r\n` terminator. Callback-based message dispatch.
- **Barcode reader** (`CBL600`, `lp204.cc:1312-1337`): Keyence BL-600. Serial at 9600/7E1. Commands: `MotorON`, `MotorOFF`, `ReadBarCode`, `Lock`. Dual-semaphore: `semCmdACK` + `semEvtRead`.
- **Hermos RFID** (`CHermos`, `lp204.cc:1357-1387`): Serial at 19200/8E1. Frame: `S|LEN|ADR|CMD|INFO|CSh|CSl|XCSh|XCSl|CR`. Dual checksum (addition + XOR). Commands: `ReadRFID`, `ReadMULTIPAGE`, `WriteRFID`, `AskVersion`. Error code in `m_failCode[2]`.
- **Omron RFID** (`COmron`, `lp204.cc:1415-1444`): Serial at 9600/8E1. Frame: `CC|PARAM|CR`. Completion code "00" = success. Supports ASCII and HEX content formats. Page bitmap encoding for read commands.
- **E84 Digital I/O** (`CE84`, `lp204.cc:1470-1523`): SeaIo GPIO. Output bits: L_REQ(0), U_REQ(1), READY(3), HO_AVBL(6), ES(7). Input: VALID, CS_0, CS_1, LTCIN, TR_REQ, BUSY, COMPT, CONT. Semaphore-guarded output writes.
- **N2 Purge**: Nozzle up/down via TAS300 (`BPNUP`/`BPNDW`). Status via `GET:PGSTA`. Configurable `_N2PurgeNozzleDown_InE84` flag.
- **Light curtain**: Via E84 cross-port I/O (`SetLTC`, `SetLTC_LED`). Modes: 0=disabled, 1=in-transfer, 2=always.

### Dual-Port Architecture

Two CLP instances (LP1, LP2) cross-linked via `m_brother` pointer (`lp204.cc:1540`). Shared `CConfig` object. E84 ports: LP1 writes to dio2/reads dio0; LP2 writes to dio3/reads dio1. Light curtain control requires cross-port coordination between siblings.

### Carrier ID Reader Configuration

Runtime-configurable via `CConfig` (`lp204.cc:28-29`): `1`=Keyence barcode, `2`=Hermos RFID, `3`=Omron ASCII, `4`=Omron HEX. Config string format: `"1h2h:En0Lv1"` encoding both LP readers and LTC settings. Persisted to file; ID reader changes require restart.

## Security

- `lp204.cc` contains `system()` calls for `shutdown`, `date`, `make`, `rm`, and `mv` commands. The C# re-implementation must **not** expose shell execution. Use `Process.Start` with argument arrays or managed APIs instead.
- Host commands arrive over serial and are parsed token-by-token with fixed-size buffers. The C# version should use managed strings with length validation at the protocol boundary.
- Log directory cleanup (`LogMaintainThread`) uses `system("rm -f ...")` in C++. C# should use `System.IO.File.Delete`.
