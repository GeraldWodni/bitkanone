compiletoflash

: invert  not  inline 1-foldable ;

: count dup c@ ; ( cstr-addr -- cstr-addr count )

: MOTD cr
." Welcome back, commander!" cr ;

$400253FC constant PORTF_DATA ( Ein- und Ausgaberegister )  
$40025400 constant PORTF_DIR  ( Soll der Pin Eingang oder Ausgang sein ? )
$40025500 constant PORTF_DR2R ( 2 mA Treiber )
$40025504 constant PORTF_DR4R ( 4 mA )
$40025508 constant PORTF_DR8R ( 8 mA )
$4002550C constant PORTF_ODR  ( Open Drain )
$40025510 constant PORTF_PUR  ( Pullup Resistor )
$40025514 constant PORTF_PDR  ( Pulldown Resistor )
$40025518 constant PORTF_SLR  ( Slew Rate )
$4002551C constant PORTF_DEN  ( Digital Enable )

decimal

: init
  \ PF0 ist auch der NMI-Eingang. Benötige also eine besondere Sequenz, um ihn für den Taster freizuschalten.
  $4C4F434B $40025520 !    ( PORTF_LOCK )
          1 $40025524 bis! ( PORTF_CR )
          0 $40025520 !    ( PORTF_LOCK )

  %11111 portf_den ! \ Alle Leitungen an Port F seien digitale Pins
  %01110 portf_dir ! \ Die Leuchtdiodenanschlüsse seien Ausgänge
  %10001 portf_pur ! \ Hochziehwiderstände für die Taster aktivieren

  cr
  MOTD
;

: bounds over + swap ;

: str-bounds dup c@ bounds 1+ swap 1+ swap ;

: cornerstone ( Name ) ( -- )
  <builds begin here $3FF and while 0 h, repeat
  does>   begin dup  $3FF and while 2+   repeat 
          eraseflashfrom
;

: hex. ." $" hex u. decimal ;

: -rot rot rot ;
: create <builds does> ;

: limits ( n1 n-min n-max -- n2 )
	rot min max ;

