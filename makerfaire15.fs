\ Demos for Maker Faire 2015 in Hannover     uh 2015-06-07

: rainbow ( -- )
     red 100 ms   blue 100 ms   green 100 ms 
     yellow 100 ms   white 100 ms  off 100 ms ;

: -rainbow ( -- )
     white 100 ms  yellow 100 ms  green 100 ms
     blue 100 ms   red 100 ms ;

: rb ( -- )
    BEGIN 
      rainbow  -rainbow
      key?
    UNTIL ;

: set-blue ( n -- )  leds n-leds 100 ms ;

: blues ( -- )  
    $20 0 do      I   set-blue  loop 
    $20 0 do  $20 I - set-blue  loop ;

: set-red ( n -- ) 16 lshift  leds n-leds 100 ms ;

: reds ( -- )  
    $20 0 do      I   set-red  loop 
    $20 0 do  $20 I - set-red  loop ;

: blitz   on 10 ms   off 10 ms ;

: rgb 
       clear s" \grot"  markup flush 1000 ms 
       clear s" \bgruen"  markup flush 1000 ms 
       clear s" \yblau"  markup flush 1000 ms 
       clear s" \rgelb"  markup flush 1000 ms  
;

: game
     begin
       clear s" \w       Sag die Farben, die leuchten." >m-scroll
       1000 ms
       7 0 do  rgb  loop 
       key?
    until ;

decimal


: redbar! ( i -- )
    8 0 do $7F0000 over i xy! loop drop ; 

: blackbar! ( i -- )
    8 0 do $000000 over i xy! loop drop ; 

decimal

: redbar 
     30 0 do  I redbar!  flush I blackbar! flush  loop ;

: run   begin dup execute key? until ;

: arrow ( color i -- )
    2dup  3 xy!
    2dup  4 xy!
    1+
    2dup  2 xy!
    2dup  5 xy!
    1+
    2dup 1 xy!
    2dup 6 xy!
    1+
    2dup 0 xy!
         7 xy! ;

: red-arrow ( i -- )
    $FF0000 swap arrow ;

: black-arrow ( i -- )
   $000000 swap arrow ;

: wegweiser ( -- )
    26 0 do  26 i -  red-arrow flush  10 ms  
             26 i - black-arrow flush
         loop ;

: say  clear 0 parse  markup flush ;
: scr  clear 0 parse  markup >scroll ;

: (show-warp ( i -- ) clear
      dup 0 = if drop s" \r\2Warp 0" exit then
      dup 1 = if drop s" \r\2Warp 1" exit then
      dup 2 = if drop s" \r\2Warp 2" exit then
      dup 3 = if drop s" \r\2Warp 3" exit then
      dup 4 = if drop s" \r\2Warp 4" exit then
      drop ;

: show-warp    (show-warp  markup flush ;

: adjust ( i -- i ? )
     key 
     dup bl = if  drop    1 exit then
     dup 43 = if  drop 1+ 0 exit then
     dup 45 = if  drop 1- 0 exit then
     drop 0 ;

: warp ( -- )
    0 begin  0 max 4 min
        dup show-warp  adjust    
      until ;

