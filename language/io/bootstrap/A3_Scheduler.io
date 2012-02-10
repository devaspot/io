
setSlot("fac", method(n, if(n == (0), 1, n * (fac(n - (1))))))
setSlot("linearAsyncBlock", method(1 println; yield; 2 println; yield; 3 println; 42))
setSlot("Test", Object clone)
setSlot("yieldAll", method(while (yieldingCoros>(0), yield)))

Test do (
	setSlot("a", 10)
	setSlot("t", 10)
	setSlot("wh2", method(wh))
	setSlot("wh", method(
		while(
			t > (0),
			a print; " " print; yield; updateSlot("a", a + (1)); 
			updateSlot("t", t - (1));
			self
		)
	)
)

setSlot("x", 100000)
while (x > (0), Test clone @@(wh); setSlot("x", x - (1)) ; self)

Message shuffleOn
