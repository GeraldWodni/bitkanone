\ Markup language for text
\ (c)copyright 2014 by Gerald Wodni

text


compiletoflash

: markup-char ( x -- )
	case
		[char] r of $0F0000 text-color ! endof
		[char] g of $000F00 text-color ! endof
		[char] b of $09000F text-color ! endof
		[char] c of $080800 text-color ! endof
		[char] m of $080008 text-color ! endof
		[char] y of $080800 text-color ! endof
		[char] w of $080808 text-color ! endof

		[char] : of bold endof
		[char] . of regular endof

		[char] 1 of 8px font ! endof
		[char] 2 of 8px-cond font ! endof
	endcase
	;

: m-len ( c-addr -- )
	0 swap
	str-bounds ?do
		i c@ c-pos c@ 1+ ." L:" . cr
	loop ;

: markup ( c-addr -- )
	dup m-len .
	str-bounds ?do
		\ control sequence
		i c@ dup [char] \ = if
			i 1+ c@
			markup-char
			2
		\ normal char
		else 
			d-emit 1
		then
	+loop ;

: m( [char] ) parse markup z-flush immediate ;

: m" postpone s" postpone markup z-flush immediate ;

clear

\ m( ll)
m( \w\2h\ra\b\1al\:l\.\co)
