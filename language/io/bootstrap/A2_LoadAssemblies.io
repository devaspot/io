
CLR loadAssembly("System.Windows.Forms")
CLR loadAssembly("mscorlib")
CLR loadAssembly("System.Drawing")
CLR loadAssembly("IoVM")

CLR using("System.Windows.Forms")
CLR using("System.Drawing")
CLR using("io")
CLR using("System.Collections")

setSlot("form", Form new)
setSlot("button", Button new)
button set_Text("MyButton")
button set_Location(Point new (10,10))
form get_Controls Add(button)

Message shuffleOff
