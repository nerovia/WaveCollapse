// DELETE THIS CONTENT IF YOU PUT COMBINED GRAMMAR IN Parser TAB
lexer grammar WaveRulzLexer;

SEMI : ';' ;

PREP: '<' ('tr' | 'tl' | 'br' | 'bl' | '-' | '|' | '+' | '/' | '\\' | '+' | '*' | [ltrbx]) '>';
WS: [ \t\n\r\f]+ -> skip ;
ID: '\\' [a-zA-Z0-9]+ | .;
