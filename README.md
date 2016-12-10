# About
Naru     - Programming language for numerical calculation  
NaruTest - CLI Tool for Naru

# Usage
## Execute Directly
When type script directly, No argument required.

Windows:  
`# NaruTest.exe`

Linux, OS X:  
`# mono NaruTest.exe`

Type 'end' to execute script typed in.   
e.g.

```
function fibo(numeric n) {
  return cond([n<2],[n],[fibo(n-1)+fibo(n-2)]);
}

var N=8;
println("fibo("+N+") = "+fibo(N));
end
```

## Execute With File
When execute script from file, Specify the script file.

Windows:  
`# NaruTest.exe script.nrk`

Linux, OS X:  
`# mono NaruTest.exe script.nrk`
