\ (c)copyright 2014 by Gerald Wodni
\ Standalone GOL Animations

gol

compiletoflash

: off-break ( -- )
	gol-off
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
: gol-endless
	begin
		gol-standalone
	key? until ;

