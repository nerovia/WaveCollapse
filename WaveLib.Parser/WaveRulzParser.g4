parser grammar WaveRulzParser;
options { tokenVocab=WaveRulzLexer; }

tile: ID;

prep: PREP tile+;

stat: tile prep+ SEMI;

rulz: stat*;
