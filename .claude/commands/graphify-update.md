Run an incremental graphify update on the project root, including sensitive files after user validation.

## Step 1 — Verify graphify installed

```powershell
python -c "import graphify" 2>$null
if ($LASTEXITCODE -ne 0) { pip install graphifyy -q 2>&1 | Select-Object -Last 3 }
python -c "import sys; open('.graphify_python', 'w').write(sys.executable)"
```

## Step 2 — Detect changed files

```powershell
& (Get-Content .graphify_python) -c "
import json
from graphify.detect import detect_incremental
from pathlib import Path

result = detect_incremental(Path('.'))
Path('graphify-out/.graphify_incremental.json').write_text(json.dumps(result))
new_total = result.get('new_total', 0)
skipped = result.get('skipped_sensitive', [])
files = result.get('new_files', {})
print('new_total:', new_total)
print('skipped_sensitive:', len(skipped))
for ftype, flist in files.items():
    if flist:
        print(ftype + ':', len(flist))
"
```

If `new_total` is 0 AND no sensitive files skipped: print "Nothing changed since last run." and stop.

## Step 3 — Handle sensitive files

If `skipped_sensitive` count > 0:
- Print the full list of skipped sensitive file paths (run `python -c` to read `.graphify_incremental.json` and print them)
- **Ask the user**: "Include these N sensitive files in the graph update? (they appear to be source code, not actual credentials)"
- Wait for user answer before continuing

If user says **yes** (include):
```powershell
& (Get-Content .graphify_python) -c "
import json
from pathlib import Path

result = json.loads(Path('graphify-out/.graphify_incremental.json').read_text())
skipped = result.get('skipped_sensitive', [])
code_files = result.get('new_files', {}).get('code', [])
code_files.extend(skipped)
result['new_files']['code'] = code_files
result['new_total'] = result.get('new_total', 0) + len(skipped)
result['skipped_sensitive'] = []
Path('graphify-out/.graphify_incremental.json').write_text(json.dumps(result))
print('Merged', len(skipped), 'sensitive files. Total:', result['new_total'])
"
```

If user says **no** (exclude): proceed with sensitive files skipped (default behavior).

## Step 4 — Check if code-only

```powershell
& (Get-Content .graphify_python) -c "
import json
from pathlib import Path

result = json.loads(Path('graphify-out/.graphify_incremental.json').read_text())
new_files = result.get('new_files', {})
doc_count = len(new_files.get('document', []))
print('doc_count:', doc_count)
print('code_count:', len(new_files.get('code', [])))
"
```

- If `doc_count` is 0: **code-only path** — skip Step 5B (no semantic subagents needed)
- If `doc_count` > 0: **full path** — run both AST and semantic extraction

## Step 5A — AST extraction (always run for code files)

```powershell
& (Get-Content .graphify_python) -c "
import json
from graphify.extract import collect_files, extract
from pathlib import Path

result = json.loads(Path('graphify-out/.graphify_incremental.json').read_text())
code_files = []
for f in result.get('new_files', {}).get('code', []):
    p = Path(f)
    if p.is_dir():
        code_files.extend(collect_files(p))
    elif p.exists():
        code_files.append(p)

if code_files:
    ast_result = extract(code_files)
    Path('.graphify_ast.json').write_text(json.dumps(ast_result, indent=2))
    print('AST:', len(ast_result['nodes']), 'nodes,', len(ast_result['edges']), 'edges')
else:
    Path('.graphify_ast.json').write_text(json.dumps({'nodes':[],'edges':[],'input_tokens':0,'output_tokens':0}))
    print('No code files')
" 2>&1
```

## Step 5B — Semantic extraction (only if doc files exist)

Skip this entire step if `doc_count` from Step 4 is 0.

### Check cache first

```powershell
& (Get-Content .graphify_python) -c "
import json
from graphify.cache import check_semantic_cache
from pathlib import Path

result = json.loads(Path('graphify-out/.graphify_incremental.json').read_text())
doc_files = result.get('new_files', {}).get('document', [])
cached_nodes, cached_edges, cached_hyperedges, uncached = check_semantic_cache(doc_files)
if cached_nodes or cached_edges or cached_hyperedges:
    Path('.graphify_cached.json').write_text(json.dumps({'nodes': cached_nodes, 'edges': cached_edges, 'hyperedges': cached_hyperedges}))
Path('.graphify_uncached.txt').write_text('\n'.join(uncached))
print('Cache:', len(doc_files)-len(uncached), 'hit,', len(uncached), 'need extraction')
"
```

### Dispatch subagents for uncached docs

Split uncached files from `.graphify_uncached.txt` into chunks of 20 files each. Dispatch ALL chunks as parallel Agent tool calls in a single message.

Each subagent prompt:
```
You are a graphify extraction subagent. Read the files listed and extract a knowledge graph fragment.
Output ONLY valid JSON matching the schema below - no explanation, no markdown fences, no preamble.

Files (chunk N of TOTAL):
[FILE LIST]

Rules:
- EXTRACTED: relationship explicit in source
- INFERRED: reasonable inference
- AMBIGUOUS: uncertain

Doc files: extract named concepts, entities, citations. Store rationale as a `rationale` attribute on relevant nodes — do NOT create separate rationale nodes.

confidence_score REQUIRED on every edge:
- EXTRACTED: 1.0
- INFERRED: 0.6-0.9 based on evidence strength
- AMBIGUOUS: 0.1-0.3

Output exactly this JSON (no other text):
{"nodes":[{"id":"filestem_entityname","label":"Human Readable Name","file_type":"document","source_file":"relative/path","source_location":null,"source_url":null,"captured_at":null,"author":null,"contributor":null}],"edges":[{"source":"node_id","target":"node_id","relation":"calls|implements|references|conceptually_related_to|shares_data_with|semantically_similar_to","confidence":"EXTRACTED|INFERRED|AMBIGUOUS","confidence_score":1.0,"source_file":"relative/path","source_location":null,"weight":1.0}],"hyperedges":[{"id":"snake_case_id","label":"Label","nodes":["node_id1","node_id2","node_id3"],"relation":"participate_in|implement|form","confidence":"EXTRACTED|INFERRED","confidence_score":0.75,"source_file":"relative/path"}],"input_tokens":0,"output_tokens":0}

After outputting the JSON, also write the result to disk as: graphify-out/.graphify_chunk_NN.json
(where NN is the zero-padded chunk number, e.g. 01, 02, 03)
```

### Verify chunks written, merge semantic

After all agents complete:
1. Verify each `graphify-out/.graphify_chunk_NN.json` exists — warn on any missing
2. Merge all chunks + cached into `.graphify_semantic_new.json`:

```powershell
& (Get-Content .graphify_python) -c "
import json
from pathlib import Path

chunks = []
for p in sorted(Path('graphify-out').glob('.graphify_chunk_*.json')):
    try:
        chunks.append(json.loads(p.read_text(encoding='utf-8')))
    except Exception as e:
        print('Warning: failed to parse', p.name, '-', e)

all_nodes = [n for c in chunks for n in c.get('nodes', [])]
all_edges = [e for c in chunks for e in c.get('edges', [])]
all_hyper = [h for c in chunks for h in c.get('hyperedges', [])]
Path('.graphify_semantic_new.json').write_text(json.dumps({'nodes': all_nodes, 'edges': all_edges, 'hyperedges': all_hyper, 'input_tokens': 0, 'output_tokens': 0}))
print('New semantic:', len(all_nodes), 'nodes,', len(all_edges), 'edges')
"
```

3. Save to cache:

```powershell
& (Get-Content .graphify_python) -c "
import json
from graphify.cache import save_semantic_cache
from pathlib import Path

new = json.loads(Path('.graphify_semantic_new.json').read_text())
saved = save_semantic_cache(new.get('nodes', []), new.get('edges', []), new.get('hyperedges', []))
print('Cached', saved, 'files')
"
```

4. Merge cached + new:

```powershell
& (Get-Content .graphify_python) -c "
import json
from pathlib import Path

cached_path = Path('.graphify_cached.json')
new_path = Path('.graphify_semantic_new.json')
cached = json.loads(cached_path.read_text()) if cached_path.exists() else {'nodes':[],'edges':[],'hyperedges':[]}
new = json.loads(new_path.read_text()) if new_path.exists() else {'nodes':[],'edges':[],'hyperedges':[]}
all_nodes = cached['nodes'] + new.get('nodes', [])
all_edges = cached['edges'] + new.get('edges', [])
all_hyper = cached.get('hyperedges', []) + new.get('hyperedges', [])
seen = set()
deduped = [n for n in all_nodes if n['id'] not in seen and not seen.add(n['id'])]
Path('.graphify_semantic.json').write_text(json.dumps({'nodes': deduped, 'edges': all_edges, 'hyperedges': all_hyper, 'input_tokens': 0, 'output_tokens': 0}, indent=2))
print('Semantic total:', len(deduped), 'nodes,', len(all_edges), 'edges')
"
```

If Step 5B was skipped (code-only), create an empty semantic file:
```powershell
& (Get-Content .graphify_python) -c "
from pathlib import Path; import json
Path('.graphify_semantic.json').write_text(json.dumps({'nodes':[],'edges':[],'hyperedges':[],'input_tokens':0,'output_tokens':0}))
"
```

## Step 6 — Merge AST + semantic

```powershell
& (Get-Content .graphify_python) -c "
import json
from pathlib import Path

ast = json.loads(Path('.graphify_ast.json').read_text())
sem = json.loads(Path('.graphify_semantic.json').read_text())
seen = {n['id'] for n in ast['nodes']}
merged_nodes = list(ast['nodes'])
for n in sem['nodes']:
    if n['id'] not in seen:
        merged_nodes.append(n)
        seen.add(n['id'])
merged = {'nodes': merged_nodes, 'edges': ast['edges'] + sem['edges'], 'hyperedges': sem.get('hyperedges', []), 'input_tokens': 0, 'output_tokens': 0}
Path('.graphify_extract.json').write_text(json.dumps(merged, indent=2))
print('Extract:', len(merged_nodes), 'nodes,', len(merged['edges']), 'edges')
"
```

## Step 7 — Merge into existing graph, cluster, report

Save backup first:
```powershell
Copy-Item graphify-out/graph.json .graphify_old.json -ErrorAction SilentlyContinue
```

Build and cluster:
```powershell
$env:PYTHONIOENCODING = "utf-8"
& (Get-Content .graphify_python) -c "
import json, io, sys
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
from graphify.build import build_from_json
from graphify.cluster import cluster, score_all
from graphify.analyze import god_nodes, surprising_connections, suggest_questions
from graphify.report import generate
from graphify.export import to_json
from networkx.readwrite import json_graph
from pathlib import Path

existing_data = json.loads(Path('graphify-out/graph.json').read_text(encoding='utf-8'))
G = json_graph.node_link_graph(existing_data, edges='links')
new_extraction = json.loads(Path('.graphify_extract.json').read_text(encoding='utf-8'))
G_new = build_from_json(new_extraction)
G.update(G_new)

detection = {'total_files': 0, 'total_words': 0, 'needs_graph': True, 'warning': None, 'files': {'code': [], 'document': []}}
tokens = {'input': 0, 'output': 0}
communities = cluster(G)
cohesion = score_all(G, communities)
gods = god_nodes(G)
surprises = surprising_connections(G, communities)
labels = {cid: 'Community ' + str(cid) for cid in communities}
questions = suggest_questions(G, communities, labels)

report = generate(G, communities, cohesion, labels, gods, surprises, detection, tokens, '.', suggested_questions=questions)
Path('graphify-out/GRAPH_REPORT.md').write_text(report, encoding='utf-8')
to_json(G, communities, 'graphify-out/graph.json')

analysis = {'communities': {str(k): v for k, v in communities.items()}, 'cohesion': {str(k): v for k, v in cohesion.items()}, 'gods': gods, 'surprises': surprises, 'questions': questions}
Path('.graphify_analysis.json').write_text(json.dumps(analysis, indent=2), encoding='utf-8')
print('Graph:', G.number_of_nodes(), 'nodes,', G.number_of_edges(), 'edges,', len(communities), 'communities')
" 2>&1
```

## Step 8 — Label top communities

Read `.graphify_analysis.json`. For each of the top 20 communities by size, look at the first 5 node labels and assign a 2-5 word plain-language name. Then regenerate:

```powershell
$env:PYTHONIOENCODING = "utf-8"
& (Get-Content .graphify_python) -c "
import json, io, sys
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
from graphify.build import build_from_json
from graphify.cluster import score_all
from graphify.analyze import suggest_questions
from graphify.report import generate
from networkx.readwrite import json_graph
from pathlib import Path

G = json_graph.node_link_graph(json.loads(Path('graphify-out/graph.json').read_text(encoding='utf-8')), edges='links')
analysis = json.loads(Path('.graphify_analysis.json').read_text(encoding='utf-8'))
communities = {int(k): v for k, v in analysis['communities'].items()}
cohesion = {int(k): v for k, v in analysis['cohesion'].items()}
detection = {'total_files': 0, 'total_words': 0, 'needs_graph': True, 'warning': None, 'files': {'code': [], 'document': []}}
tokens = {'input': 0, 'output': 0}

# REPLACE THIS DICT with labels derived from community node inspection
labels = LABELS_DICT
for cid in communities:
    if cid not in labels:
        labels[cid] = 'Community ' + str(cid)

questions = suggest_questions(G, communities, labels)
report = generate(G, communities, cohesion, labels, analysis['gods'], analysis['surprises'], detection, tokens, '.', suggested_questions=questions)
Path('graphify-out/GRAPH_REPORT.md').write_text(report, encoding='utf-8')
Path('.graphify_labels.json').write_text(json.dumps({str(k): v for k, v in labels.items()}), encoding='utf-8')
print('Labels applied')
" 2>&1
```

## Step 9 — Generate HTML

```powershell
$env:PYTHONIOENCODING = "utf-8"
& (Get-Content .graphify_python) -c "
import json, io, sys
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
from graphify.export import to_html
from networkx.readwrite import json_graph
from pathlib import Path

G = json_graph.node_link_graph(json.loads(Path('graphify-out/graph.json').read_text(encoding='utf-8')), edges='links')
analysis = json.loads(Path('.graphify_analysis.json').read_text(encoding='utf-8'))
labels_raw = json.loads(Path('.graphify_labels.json').read_text(encoding='utf-8')) if Path('.graphify_labels.json').exists() else {}
communities = {int(k): v for k, v in analysis['communities'].items()}
labels = {int(k): v for k, v in labels_raw.items()}
n = G.number_of_nodes()
if n > 5000:
    print('Graph has', n, 'nodes - too large for HTML. Use Obsidian vault instead.')
else:
    to_html(G, communities, 'graphify-out/graph.html', community_labels=labels or None)
    print('graph.html written')
" 2>&1
```

## Step 10 — Save manifest, clean up

```powershell
$env:PYTHONIOENCODING = "utf-8"
& (Get-Content .graphify_python) -c "
import json, io, sys
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
from graphify.detect import save_manifest
from pathlib import Path
from datetime import datetime, timezone

result = json.loads(Path('graphify-out/.graphify_incremental.json').read_text(encoding='utf-8'))
save_manifest(result['new_files'])
cost_path = Path('graphify-out/cost.json')
cost = json.loads(cost_path.read_text(encoding='utf-8')) if cost_path.exists() else {'runs': [], 'total_input_tokens': 0, 'total_output_tokens': 0}
cost['runs'].append({'date': datetime.now(timezone.utc).isoformat(), 'input_tokens': 0, 'output_tokens': 0, 'files': result.get('new_total', 0)})
cost_path.write_text(json.dumps(cost, indent=2), encoding='utf-8')
print('Manifest saved. Runs total:', len(cost['runs']))
" 2>&1
Remove-Item -ErrorAction SilentlyContinue .graphify_extract.json, .graphify_ast.json, .graphify_semantic.json, .graphify_analysis.json, .graphify_labels.json, .graphify_semantic_new.json, .graphify_cached.json, .graphify_uncached.txt, .graphify_old.json, .graphify_python
Remove-Item -ErrorAction SilentlyContinue graphify-out/.graphify_incremental.json, graphify-out/.needs_update
Get-ChildItem graphify-out -Filter ".graphify_chunk_*.json" | Remove-Item -ErrorAction SilentlyContinue
```

## Final output

Tell the user:
```
Graph updated. Outputs in graphify-out/

  graph.html       - open in browser
  GRAPH_REPORT.md  - audit report
  graph.json       - raw graph data
```

Then paste the **God Nodes**, **Surprising Connections**, and **Suggested Questions** sections from `graphify-out/GRAPH_REPORT.md` directly into the chat.

Pick the single most interesting suggested question (most cross-community bridges) and ask: "Want me to trace it?"
