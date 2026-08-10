const fs = require('fs');
const { newDb } = require('pg-mem');

let schema = fs.readFileSync('/sessions/fervent-gallant-shannon/mnt/Estoy louco/deploy_schema_postgres.sql','utf8');
schema = schema.replace(/DROP TABLE[\s\S]*?CASCADE;/, '');

const db = newDb();
db.public.none(schema);
db.public.none("INSERT INTO produtos (nome,preco,categoria,ingredientes,ativo) VALUES ('DevClassic',25,'Lanche Tradicional','pao, carne',true),('ByteBurger',30,'Lanche Gourmet','bacon',true),('CocaCola',7,'Bebidas','',true)");
db.public.none("INSERT INTO clientes (nome,telefone) VALUES ('Teste','111')");
db.public.none("INSERT INTO pedidos (idcliente,data,total,status,tipoentrega,origem) VALUES (1, NOW(), 75, 'Finalizado','Retirada','Desktop')");
db.public.none("INSERT INTO itenspedido (idpedido,idproduto,quantidade,preco) VALUES (1,1,3,25)");

const pgAdapter = db.adapters.createPg();
require.cache[require.resolve('pg')] = { id: require.resolve('pg'), loaded: true, exports: pgAdapter };

const express = require('express');
const app = express();
app.use(express.json());
app.use('/api', require('./src/rotas/produtos'));
app.use('/api', require('./src/rotas/pedidos'));
const server = app.listen(4100, run);

function req(method, path, body){
  const http = require('http');
  return new Promise((resolve,reject)=>{
    const data = body!==undefined ? JSON.stringify(body) : null;
    const headers = {'Content-Type':'application/json'};
    if(data) headers['Content-Length'] = Buffer.byteLength(data);
    const r = http.request({host:'127.0.0.1',port:4100,path,method,headers}, res=>{
      let d='';res.on('data',c=>d+=c);res.on('end',()=>resolve({status:res.statusCode, body:d}));
    });
    r.on('error',reject); if(data) r.write(data); r.end();
  });
}

async function run(){
  try{
    let x;
    x = await req('GET','/api/produtos'); console.log('GET /produtos ->', x.status, x.body.slice(0,140));
    x = await req('GET','/api/mais-vendidos?top=3'); console.log('GET /mais-vendidos ->', x.status, x.body.slice(0,160));
    x = await req('POST','/api/pedidos',{cliente:{nome:'Ana',telefone:'99999'},tipoEntrega:'Entrega',endereco:'Rua X',bairro:'Centro',itens:[{idProduto:1,quantidade:2}]});
    console.log('POST /pedidos ->', x.status, x.body);
    let id = 0; try{ id = JSON.parse(x.body).idPedido; }catch(e){}
    if(id){ x = await req('GET','/api/pedidos/'+id+'/status'); console.log('GET /pedidos/'+id+'/status ->', x.status, x.body); }
    x = await req('POST','/api/pedidos',{}); console.log('POST /pedidos (invalido) ->', x.status, x.body.slice(0,90));
  }catch(e){ console.log('ERRO teste:', e.message); }
  server.close(); process.exit(0);
}
