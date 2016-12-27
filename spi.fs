\ SSI Module for lm4f120-MECRISP
\ (c)copyright 2013-2014 by Gerald Wodni <gerald.wodni@gmail.com>

\ PORTB
$400053FC constant PORTB_DATA   ( I/O-Data )
$40005400 constant PORTB_DIR    ( Direction )
$4000551C constant PORTB_DEN    ( Digital Enable )
$40005420 constant PORTB_AFSEL  ( Alternate function select )
$40005528 constant PORTB_AMSEL  ( Analog Mode Select )
$4000552C constant PORTB_PCTL   ( Port Control )

\ SSI Base Adresses
$40008000 constant SSI_BASE
$1000     constant SSI_OFFSET
( Module 0: PA2=CLK, PA3=FSS, PA4=RX, PA4=TX )
( Module 1: PF0=CLK, PF1=FSS, PF2=RX, PF3=TX or PD0=CLK, PD1=FSS, PD2=RX, PD3=TX )
( Module 2: PB4=CLK, PB5=FSS, PB6=RX, PB7=TX )
( Module 3: PD0=CLK, PD1=FSS, PD2=RX, PD3=TX )

\ SSI Register Offsets
$000 constant SSI_CR0           ( Control 0 )
$004 constant SSI_CR1           ( Control 1 )
$008 constant SSI_DR            ( Data )
$00C constant SSI_SR            ( Status )
$010 constant SSI_CPSR          ( Clock Prescale 2-254 )
$014 constant SSI_IM            
$018 constant SSI_RIS
$01C constant SSI_MIS
$020 constant SSI_ICR
$024 constant SSI_DMACTL 
$FC8 constant SSI_CC            ( Clock Control )

\ SSI bits
$10 constant SSI_CR1_EOT        ( End of Transmission )
$08 constant SSI_CR1_SOD        ( Slave Mode Output Disable )
$04 constant SSI_CR1_MS         ( Master/Slave Select: 0 = Master, 1 = Slave )
$02 constant SSI_CR1_SSE        ( Port Enable )
$01 constant SSI_CR1_LBM        ( Loopback Mode )

$FF00 constant SSI_CR0_SCR_M    ( SSI Serial Clock Rate )
$0080 constant SSI_CR0_SPH      ( SSI Serial Clock Phase )
$0040 constant SSI_CR0_SPO      ( SSI Serial Clock Polarity )
$0030 constant SSI_CR0_FRF_M    ( SSI Frame Format Select )
$0000 constant SSI_CR0_FRF_MOTO ( Freescale Frame Format )
$0010 constant SSI_CR0_FRF_TI   ( TI Frame Format )
$0020 constant SSI_CR0_FRF_NMW  ( MICROWIRE Frame Format )
$000F constant SSI_CR0_DSS_M    ( SSI Data Size Select )

$10 constant SSI_SR_BSY         ( SSI Busy Bit )
$08 constant SSI_SR_RFF         ( SSI Receive FIFO Full )
$04 constant SSI_SR_RNE         ( SSI Receive FIFO Not Empty )
$02 constant SSI_SR_TNF         ( SSI Transmit FIFO Not Full )
$01 constant SSI_SR_TFE         ( SSI Transmit FIFO Empty )
  
( Synchronous Serial Interface )
$400FE61C constant RCGCSSI      ( SSI  Run Mode Clock Gating Control )
$400FE608 constant RCGCGPIO     ( GPIO Run Mode Clock Gating Control )

: ssi-base ( u-ssi-number -- addr-offset ) inline
    SSI_OFFSET * SSI_BASE + ;

: ssi-offset ( u-ssi-number addr-offset -- addr-register ) inline
    swap ssi-base + ;

: enable-ssi ( u-ssi-number -- )
    SSI_CR1 ssi-offset
    SSI_CR1_SSE swap bis! ;

: disable-ssi ( u-ssi-number -- )
    SSI_CR1 ssi-offset
    SSI_CR1_SSE swap bic! ;

\ bitmask
: bm ( u-bit -- u-mask ) inline
    1 swap lshift ;

0 variable ssi-dr               ( current ssi-data-register, used by ssi-i/o functions )
0 variable ssi-sr               ( current ssi-status-register )
: set-ssi ( n -- )              ( set current ssi )
    >r r@ SSI_DR ssi-offset ssi-dr !
    r> SSI_SR ssi-offset ssi-sr ! ;

: init-ssi ( u-flags u-data-size u-ssi-number -- )
    >r r@ set-ssi               ( set as current ssi to be used by future i/o )
    r@ bm RCGCSSI !             ( Enable SSI Registers )
    r@ disable-ssi              ( Disable SSI during setup )
    0 r@ SSI_CPSR ssi-offset !  ( Use System Clock )
    1- $F and or                ( Merge Datasize[0-F] and flags )
    r> SSI_CR0 ssi-offset ! ;

: ssi-speed ( u-clock-rate u-prescaler u-ssi-number -- )
    >r 				\ BR=SysClk/(CPSDVSR * (1 + SCR))
    5 r@ SSI_CC ssi-offset !    ( Use PIOSC )
    r@ SSI_CPSR ssi-offset !    ( Set Prescaler )
    r> SSI_CR0 ssi-offset >r    ( Store CR0-Address )
    $FF and 8 lshift            ( shift Clock-Rate to correct bits )
    r@ @ or r> ! ;              ( Set Clock-Rate in CR0 )

: >ssi ( x-tx -- ) inline
    ssi-dr @ ! ;

: ssi> ( -- x-rx ) inline
    ssi-dr @ @ ;

: >ssi> ( x-tx -- x-rx ) inline
    >ssi ssi> ;

: ssi-status ( -- x-status ) inline
    ssi-sr @ @ ;

: ssi-wait-ready ( -- )
    begin
        ssi-status SSI_SR_BSY and 0=
    until ;

: ssi-send ( c-addr n -- )
    0 do
        \ ssi-wait-ready
	dup c@ >ssi
	1+
    loop drop ;

: ssi-wait-tx-fifo ( -- )
    begin
        ssi-status SSI_SR_TFE and 0<>
    until ;

cornerstone rewind-ssi

