\ cold


compiletoflash

\ Analog-Digital-Converter
\   needs basisdefinitions.txt

$400FE638 constant RCGCADC

: init-analog ( -- ) 
  $1 RCGCADC ! \ Provide clock to AD-Converter
               \ PIOs already activated in Core
  50 0 do loop \ Wait a bit
;

$40038FC8 constant ADC0_CC   \ Clock configuration
$40038000 constant ADC0_ACTSS \ Active Sample Sequencer
$40038044 constant ADC0_SSCTL0 \ Sample Sequence Control 0
$40038048 constant ADC0_SSFIFO0 \ Sample Sequence Result FIFO
$40038040 constant ADC0_SMUX0    \ Sample Sequence Input Multiplexer Select 0
$40038028 constant ADC0_PSSI      \ Processor Sample Sequence Initiate

: temperature ( -- Measurement )
  1 ADC0_CC !      \ Select PIOSC
  0 ADC0_ACTSS !   \ Disable Sample Sequencers
  0 ADC0_SMUX0 !   \ Select input channel for first sample
 $A ADC0_SSCTL0 !  \ First Sample is from Temperature Sensor and End of Sequence
  1 ADC0_ACTSS !   \ Enable Sample Sequencer 0
  1 ADC0_PSSI !    \ Initiate sampling

  begin $10000 ADC0_ACTSS bit@ not until \ Check busy Flag for ADC

  ADC0_SSFIFO0 @ \ Fetch measurement result
;

: random ( -- u )
 ( Generiert Zufallszahlen mit dem Rauschen vom Temperatursensor am ADC )
 ( Random numbers with noise of temperature sensor on ADC )
   0
   32 0 do
     shl
    temperature 1 and
    xor
  loop
;

: random-bits ( n -- u )
	0 swap 0 do
		shl
		temperature 1 and
		xor
	loop
;

: bitcount ( x -- n-bits-required )
	0 swap begin
		swap 1+ swap	\ count bit
		1 rshift dup	\ right-shift
	0= until drop ; 	\ count until zero

: urandom ( u-max -- u )
	begin
		>r r@ r@ bitcount	\ get smallest matching bitcount
		random-bits		\ generate new value
		dup r> >=		\ continue if too big
	while
		drop			\ too big, drop and try again
	repeat nip ;

cols buffer: drops

: seed
	drops cols bounds do 
		6 random-bits i c!
	loop ;

: draw-drops
	cols 0 do
		$7f007F
		i		\ x
		drops i + c@	\ y
		xy!
	loop ;

: raindrop			\ decay all drops
	drops cols bounds do
		i c@ 1+
			dup 64 > if
				drop 0
			then
		i c!
	loop ;

: decay
	led-buffer led-buffer-size bounds do
		i c@ dup 0<> if
			1-
		then
		i c!
	loop ;

: single-rain
	draw-drops
	8 0 do decay flush 5 ms loop
	raindrop ;

: rain ( n -- )
	begin
		single-rain
	key? until ;

	
: init-rain 
	init-analog \ Enable Clock for AD-Converter
	buffer-off
	seed
	draw-drops
	rain
	;

: init init init-rain ;
