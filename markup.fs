\ Markup language for text
\ (c)copyright 2014 by Gerald Wodni

text



compiletoflash

\ create colors
\ char g c, $000000 ,

: markup-char ( x -- )
	case
		[char] r of $2F0000 text-color ! endof
		[char] g of $002F00 text-color ! endof
		[char] b of $00002F text-color ! endof
		[char] c of $002828 text-color ! endof
		[char] m of $280028 text-color ! endof
		[char] y of $282800 text-color ! endof
		[char] w of $282828 text-color ! endof

		[char] : of bold endof
		[char] . of regular endof

		[char] 1 of 8px font ! endof
		[char] 2 of 8px-cond font ! endof
	endcase
	;

: m-length ( c-addr n -- )
	0 -rot
	bounds ?do
		i c@ c-pos c@ 1+ ." L:" . cr
	loop ;

: m-length ( c-addr n -- )
	0 -rot
	bounds ?do
		\ control sequence
		i c@ dup [char] \ = if
			drop \ drop backspace
			i 1+ c@
			markup-char
			2
		\ normal char, get length
		else 
			\ dup emit
			c-pos c@ 
			\ ."  L:" dup . cr
			1+ boldness @ * 
			+
			1
		then
	+loop ;

: markup ( c-addr n -- )
	bounds ?do
		\ control sequence
		i c@ dup [char] \ = if
			drop \ drop backspace
			i 1+ c@
			markup-char
			2
		\ normal char
		else 
			d-emit 1
		then
	+loop ;

: m( [char] ) parse markup flush immediate ;

: m" postpone s" postpone markup flush immediate ;

: >m-scroll ( c-addr n -- )
	2dup m-length 1+ 0 do
		i 0 offset-column
		clear
		2dup markup flush
		100 ms
	loop 2drop ;

: ms( [char] ) parse >m-scroll immediate ;

: x s" \w\2h\ra\b\1al\:l\.\co" ;

: mcr ( c-addr n -- )
	2dup type cr m-length cr . cr ;

: b $FFFFFF buffer! flush 10 ms off ;

: bb 0 do b 100 ms loop ;

: pause begin
	\ s"            \1\rProgramm: \w14:30 \yWanderung nach zum Heurigen nach Soosz, \w18:00 \yAbendessen im Heurigen \cKrenn \gHauptstraße 76 \m2504 Soosz"
	s" \.           \yBaK\wi\yp\w\:8\. \rpresents \mMUSIC \cOF \mTHE \cNIGHT \rpowered by \wdas-Salzamt.at \rhave \m\:F\yU\cN\w!"
	>m-scroll
	." Press any key
	?key if
		key [char] p =
	else
		false
	then until ;

: test
	clear
	x markup flush
	s" h" mcr
	s" \w\2h" mcr
	s" \w\2\rh" mcr
	s" \w\2\rh\ra" mcr
	s" \w\2\rh\ra\b\1a" mcr
	s" \w\2\rh\ra\b\1al" mcr
	x >m-scroll
	;


cornerstone rewind-markup
