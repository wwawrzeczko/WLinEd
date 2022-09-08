all: wlined.exe
CSC=mcs
wlined.exe: wlined.cs
	$(CSC) wlined.cs
test: wlined.exe
	(cd examples; $(MAKE))
clean:
	rm -f wlined.exe
	(cd examples; $(MAKE) clean)
	




