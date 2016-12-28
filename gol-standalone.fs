\ (c)copyright 2014 by Gerald Wodni
\ Standalone GOL Animations

\ gol

compiletoflash

: off-break ( -- )
	buffer-off
	$000007 leds n-leds
	1000 ms ;

: step-break ( n-steps -- )
	gol-steps off-break ;
	
: gol-standalone ( -- )
	\ init-delay
	\ init-ws
	\ 10 gol-line
	\ 20 gol-steps
	glider		50 step-break
	acorn		50 step-break
	die-hard	50 step-break
	10 gol-line	50 step-break
	;

: key-flush ( -- )
	begin key? while key drop repeat ;

: gol-endless
	begin
		gol-standalone
	key? until ;

: init init 
	1000 ms
	key-flush
	off 100 ms $003F00 >rgb 
	5000 ms
	key? invert if
		gol-endless
	else
		key-flush
		$3F0000 >rgb
		." Human presence detected" cr
	then ;

