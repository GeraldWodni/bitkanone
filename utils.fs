\ (c)copyright 2014 by Gerald Wodni
\ partly taken from mecrisp examples by Mathias Koch

: MOTD ." Welcome back, commander!" ;

: init
  decimal
  cr MOTD cr ;

\ missing Forth 200x words

: invert  not  inline 1-foldable ;

: create <builds does> ;


\ helpers

: -rot rot rot ;

: count dup c@ ; ( cstr-addr -- cstr-addr count )

: hex. ." $" hex u. decimal ;

: bounds over + swap ;

: limits ( n1 n-min n-max -- n2 )
	rot min max ;

: str-bounds dup c@ bounds 1+ swap 1+ swap ;

: cornerstone ( Name ) ( -- )
  <builds begin here $3FF and while 0 h, repeat
  does>   begin dup  $3FF and while 2+   repeat 
          eraseflashfrom
;

: key-flush ( -- )
	begin key? while key drop repeat ;

: free-ram
	compiletoram
	flashvar-here here - u. ;

: free-flash
	compiletoflash
	$40000 here - u. ;

: between ( n-val n-min n-max-1 -- f )
	>r over <=
	swap r> < and ;
