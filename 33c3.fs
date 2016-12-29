\ (c)copyright 2016 by Gerald Wodni
\ 32C3 Hamburg greeting

\ very-cold


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
        forth flush 5000 ms
        around
        forth flush 5000 ms ;
	
: ev ( -- )

glider		50 step-break
separator
s" \1      \r\1\:33C3" >m-scroll
s" \1      \.\w\2Hello, I am the \rb\yi\gt\ck\ba\mn\ro\yn\ge " >m-scroll
s" \1      \.\w\2I consist of a \rTiva-LaunchPad\w and a lot of \rL\ge\bd\ws with built in \rWS2812B chip. " >m-scroll
s" \1      \.\w\2What makes me special is the fact, that my \rcompiler\w runs on the \cChip \yitself\w! " >m-scroll

acorn		50 step-break
separator
s" \1      \.\w\2I am programmed in \rForth\w, which is \cprogramming language \wand \yOperating System\w at once " >m-scroll
s" \1      \.\r\2Forth \wexists for quite a while, since ca \g1970 " >m-scroll
s" \1      \.\r\2Forth \walways has \ccompiler \won board and is \rstack-based " >m-scroll
s" \1      \.\w\2This alows \rForth\w to extend \yitself within itself\w, one can even invent \mnew syntax elements \w " >m-scroll

10 gol-line	50 step-break
separator
s" \1      \.\wIt's quite likely that there is a \r\2Forth \wsystem for your \yfavourite controller! " >m-scroll
s" \1      \.\w\2If you want to know more, ask one of the \rcarbon units \won this table " >m-scroll
	;

\ ev - endless
: eve ( -- )
	begin
		ev
	key? until ;

: init init 
	1000 ms
	key-flush
	off 100 ms $003F00 >rgb 
	5000 ms
	key? invert if
		eve
	else
		key-flush
		$3F0000 >rgb
		." Human presence detected" cr
	then ;
