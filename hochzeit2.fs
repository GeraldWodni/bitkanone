
\ (c)copyright 2014 by Gerald Wodni
\ Standalone GOL Animations

gol

compiletoflash

: off-break ( -- )
	buffer-off
	$000007 leds n-leds
	1000 ms ;

: step-break ( n-steps -- )
	gol-steps off-break ;

: black-break
	off 1500 ms ;

: separator
	black-break
	clear s" \1\.\c *\y\2 \1*\m\2 \1*" markup z-flush 2500 ms
	black-break ;
	
: gol-standalone ( -- )
	\ init-delay
	\ init-ws
	\ 10 gol-line
	\ 20 gol-steps

glider		50 step-break
separator
s" \1      \w\2Hochzeit \1\rClaudia \w& \cGerald " >m-scroll
s" \1      \.\r\2Schoen, dass ihr alle da seid \w\1;)

acorn		50 step-break
separator
s" \1      \.\w\2Fotos" >m-scroll
s" \1      \.\r\2Hinter dem \wBrauttisch \rgibt es einen Stand zum \ySelbstknipsen" >m-scroll

die-hard	50 step-break
separator
s" \1      \.\w\2Programm (ca.)" >m-scroll
clear s" \2\r\.\1 19:30" markup z-flush 2500 ms
s" \1      \.\w\2Tanzbeginn" >m-scroll
clear s" \2\r\.\1 21:30" markup z-flush 2500 ms
s" \1      \.\w\2Torte\r\1!\y!\m!" >m-scroll
clear s" \2\r\.\1 00:15" markup z-flush 2500 ms
s" \1      \.\rD\yI\gS\cC\mO" >m-scroll

10 gol-line	50 step-break
separator
s" \1      \.\w\2WCs" >m-scroll
s" \1      \.\r\2Beim \yBuffet \rdie Stiegen hinunter" >m-scroll



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

init
