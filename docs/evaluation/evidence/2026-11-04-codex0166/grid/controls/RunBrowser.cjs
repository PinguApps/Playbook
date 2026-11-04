const {spawn}=require('node:child_process');
const fs=require('node:fs');
const app=process.argv[2],port=process.argv[3],output=process.argv[4];
const log=fs.openSync(output+'.server.log','w');
const server=spawn('C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe',[app,'--urls',`http://127.0.0.1:${port}`],{stdio:['ignore',log,log],windowsHide:true,env:{...process.env,DOTNET_ADD_GLOBAL_TOOLS_TO_PATH:'0',DOTNET_GENERATE_ASPNET_CERTIFICATE:'false'}});
(async()=>{try{
 for(let i=0;i<100;i++){try{if((await fetch(`http://127.0.0.1:${port}/`)).ok)break}catch{}await new Promise(r=>setTimeout(r,100))}
 const stream=fs.openSync(output,'w');
 const browser=spawn(process.execPath,[__dirname+'/Browser.cjs',`http://127.0.0.1:${port}/`],{stdio:['ignore',stream,stream],windowsHide:true});
 const code=await new Promise(r=>browser.on('exit',r));fs.closeSync(stream);process.exitCode=code;
}finally{server.kill();fs.closeSync(log)}})().catch(e=>{console.error(e);process.exitCode=1});
