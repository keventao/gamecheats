const fs = require('fs');
const path = require('path');

const repo = path.resolve(__dirname, '..', '..');
const scriptPath = path.join(repo, 'spacehaven', 'tools', 'build_resource_yield_mod.py');
const source = fs.readFileSync(scriptPath, 'utf8');

function assertIncludes(text, needle, message) {
  if (!text.includes(needle)) {
    throw new Error(`${message}: missing ${needle}`);
  }
}

function assertRegex(text, regex, message) {
  if (!regex.test(text)) {
    throw new Error(`${message}: ${regex}`);
  }
}

assertIncludes(source, 'import copy', 'generator must deep-copy source XML products before patching');
assertIncludes(source, 'patched = copy.deepcopy(product)', 'source XML tree must not be mutated in place');
assertIncludes(source, 'def speed_up_crop_stages', 'crop speed feature exists');
assertIncludes(source, 'def increase_need_intervals', 'input saver feature exists');
assertIncludes(source, 'def mod_folder(multiplier: int)', 'folder name must vary by multiplier');
assertIncludes(source, 'def mod_id(multiplier: int)', 'mod id must vary by multiplier');
assertIncludes(source, '--no-output-boost', 'output boost can be disabled for focused tests');
assertIncludes(source, '--no-crop-speed', 'crop speed can be disabled for focused tests');
assertIncludes(source, '--no-crop-input-saver', 'crop input saver can be disabled for focused tests');
assertIncludes(source, '--no-process-input-saver', 'process input saver can be disabled for focused tests');
assertRegex(source, /if args\.multiplier < 1 or args\.multiplier > 10:/, 'multiplier bounds are enforced');
assertRegex(source, /if args\.crop_time_divisor < 1 or args\.crop_time_divisor > 10:/, 'crop time divisor bounds are enforced');
assertRegex(source, /product_type not in \{"Crop", "Process"\}/, 'only Crop and Process products are copied');

const readmePath = path.join(repo, 'spacehaven', 'README.md');
const readme = fs.readFileSync(readmePath, 'utf8');
assertIncludes(readme, 'tools/build_resource_yield_mod.py', 'README documents generator');
assertIncludes(readme, 'generated/', 'README documents generated output');

console.log('spacehaven generator static checks passed');
