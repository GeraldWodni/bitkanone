\ (c)copyright 2015 by Gerald Wodni
\ Maker Faire Hannover greeting

very-cold


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
	clear s" \1\.\c *\y\2 \1*\m\2 \1*" markup flush 2500 ms
	black-break ;
	
: ev ( -- )

glider		50 step-break
separator
s" \1      \r\1\:Maker Faire\.\1\c Hannover " >m-scroll
s" \1      \.\w\2Hallo, ich bin die \rb\yi\gt\ck\ba\mn\ro\yn\ge " >m-scroll
s" \1      \.\w\2Ich bestehe aus einem \rTiva-LaunchPad\w und viele \rL\ge\bd\ws mit eingebautem \rWS2812B chip. " >m-scroll
s" \1      \.\w\2Das eigentlich besondere an mir ist aber, dass mein \rCompiler\w auf dem \cChip \yselbst\w sitzt! " >m-scroll

acorn		50 step-break
separator
s" \1      \.\w\2Ich wurde in \rForth\w programmiert, das ist \cProgrammiersprache \wund \yBetriebssystem\w zugleich" >m-scroll
s" \1      \.\r\2Forth \wgibt es schon voll lang, seit ca \g1970 " >m-scroll
s" \1      \.\r\2Forth \what immer einen \cCompiler \wmit an Board und ist \rstackbasierend " >m-scroll
s" \1      \.\w\2Dadurch kann man \rForth\w in sich \yselbst erweitern\w, ja sogar neue \mSyntaxelemente \werfinden " >m-scroll

10 gol-line	50 step-break
separator
s" \1      \.\r\2Forth \wgibt es ziemlich wahrscheinlich auch auf deinem \yLieblingscontroller! " >m-scroll
s" \1      \.\w\2Um mehr zu erfahren, frage doch einfach eine der \rKohlenstoffeinheiten\w in meiner Umgebung " >m-scroll
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

init
