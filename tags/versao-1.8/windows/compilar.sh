mkbundle2 --deps MensagemWeb.exe
windres.exe -i resfile.rc -o prjicon.o
gcc -o mw.exe -Wall temp.c -lz temp.o prjicon.o -mno-cygwin -Ic:/ARQUIV~1/MONO-1~1.1/include -Ic:/ARQUIV~1/MONO-1~1.1/include/glib-2.0 -Ic:/ARQUIV~1/MONO-1~1.1/lib/glib-2.0/include  -mno-cygwin -Lc:/ARQUIV~1/MONO-1~1.1/lib -lmono -lm -lgmodule-2.0 -lgthread-2.0 -lglib-2.0 -lintl -liconv