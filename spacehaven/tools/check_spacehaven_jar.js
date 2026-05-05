const fs = require('fs');
const os = require('os');
const path = require('path');
const childProcess = require('child_process');

const jarPath = process.argv[2] || '<SPACEHAVEN_GAME_ROOT>/spacehaven.jar';
if (!fs.existsSync(jarPath)) {
  throw new Error(`spacehaven.jar not found: ${jarPath}`);
}

const result = childProcess.spawnSync('tar', ['-xOf', jarPath, 'library/haven'], {
  encoding: 'utf8',
  maxBuffer: 64 * 1024 * 1024,
});
if (result.status !== 0) {
  throw new Error(`tar failed: ${result.stderr || result.stdout}`);
}
const haven = result.stdout;
const productCount = (haven.match(/<product\b/g) || []).length;
const cropCount = (haven.match(/type="Crop"/g) || []).length;
const processCount = (haven.match(/type="Process"/g) || []).length;
const outputs = (haven.match(/<products>[\s\S]*?<\/products>/g) || []).length;
const consumeEvery = (haven.match(/consumeEvery="\d+"/g) || []).length;

if (productCount <= 0) throw new Error('no Product definitions found');
if (cropCount <= 0) throw new Error('no Crop products found');
if (processCount <= 0) throw new Error('no Process products found');
if (outputs <= 0) throw new Error('no product output blocks found');
if (consumeEvery <= 0) throw new Error('no consumeEvery attributes found');

console.log(`spacehaven jar check passed: products=${productCount} crop=${cropCount} process=${processCount} outputBlocks=${outputs} consumeEvery=${consumeEvery}`);
