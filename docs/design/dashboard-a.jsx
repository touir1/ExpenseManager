// Dashboard A — Hearth (recommended). Warm, editorial, family-first.

// === Shared shell used by all 3 dashboards ===
function AppNav({ ink = Hearth.ink, bg = Hearth.paper, border = Hearth.border, accent = Hearth.clay, activeBg = Hearth.claySofter, activeColor = Hearth.clayDeep, accentInk = Hearth.clay, family = "The Rivera family" }) {
  const navStyle = (active) => ({
    ...sx.sans, fontSize: 14, fontWeight: 500, color: active ? activeColor : ink, padding: '8px 14px', borderRadius: 10,
    background: active ? activeBg : 'transparent', cursor: 'pointer', display: 'inline-flex', alignItems: 'center', gap: 8,
  });
  return (
    <div style={{
      display: 'flex', alignItems: 'center', justifyContent: 'space-between',
      padding: '16px 32px', borderBottom: `1px solid ${border}`, background: bg,
    }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 32 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <HearthMark size={28} bg={accentInk} />
          <Wordmark size={17} ink={ink} accent={accentInk} />
        </div>
        <div style={{ display: 'flex', gap: 2 }}>
          <span style={navStyle(true)}>This month</span>
          <span style={navStyle(false)}>Expenses</span>
          <span style={navStyle(false)}>Budgets</span>
          <span style={navStyle(false)}>Family</span>
          <span style={navStyle(false)}>Reports</span>
        </div>
      </div>
      <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
        {/* Family selector */}
        <div style={{
          display: 'inline-flex', alignItems: 'center', gap: 8,
          padding: '6px 10px 6px 6px', background: Hearth.card, border: `1px solid ${border}`,
          borderRadius: 999,
        }}>
          <span style={{ display: 'flex' }}>
            <Avatar name="Maya R" color={Hearth.clay} size={22} />
            <span style={{ marginLeft: -6 }}><Avatar name="Jonah R" color={Hearth.sage} size={22} /></span>
            <span style={{ marginLeft: -6 }}><Avatar name="Iris" color={Hearth.mustard} size={22} /></span>
          </span>
          <span style={{ ...sx.sans, fontSize: 13, fontWeight: 600, color: ink }}>{family}</span>
          <svg width={14} height={14} viewBox="0 0 24 24" fill="none"><path d="M7 10l5 5 5-5" stroke={ink} strokeWidth="2" strokeLinecap="round"/></svg>
        </div>
        <button style={{
          ...sx.sans, fontWeight: 600, fontSize: 14, padding: '9px 16px', cursor: 'pointer',
          background: accent, color: '#fff', border: 'none', borderRadius: 999,
          display: 'inline-flex', alignItems: 'center', gap: 8,
          boxShadow: `0 6px 16px -6px ${accent}99`,
        }}><Icon.plus size={14} /> Add expense</button>
        <span style={{ color: Hearth.mute, cursor: 'pointer' }}><Icon.bell size={18} /></span>
        <Avatar name="Maya R" color={Hearth.clay} size={32} />
      </div>
    </div>
  );
}

// Soft bar+line chart for monthly spend
function SpendChart({ accent = Hearth.clay, soft = Hearth.claySoft }) {
  const data = [
    1980, 2120, 2450, 2310, 2780, 2640, 2510, 2890, 2430,
  ];
  const labels = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep'];
  const max = 3200;
  const W = 640, H = 220, gap = 18;
  const barW = (W - gap * (data.length - 1)) / data.length;
  return (
    <svg viewBox={`0 0 ${W} ${H + 40}`} width="100%" preserveAspectRatio="none" style={{ display: 'block' }}>
      {/* budget line */}
      <line x1={0} x2={W} y1={H - (2800/max)*H} y2={H - (2800/max)*H} stroke={Hearth.border} strokeDasharray="4 4" />
      <text x={6} y={H - (2800/max)*H - 6} fontFamily={sx.sans.fontFamily} fontSize="10" fill={Hearth.mute}>budget €2,800</text>

      {data.map((v, i) => {
        const h = (v / max) * H;
        const x = i * (barW + gap);
        const y = H - h;
        const isCurrent = i === data.length - 1;
        return (
          <g key={i}>
            <rect x={x} y={y} width={barW} height={h}
              fill={isCurrent ? accent : soft}
              rx={6} />
            {isCurrent && (
              <text x={x + barW/2} y={y - 8} fontFamily={sx.serifDisplay.fontFamily} fontSize="16" fill={Hearth.ink} textAnchor="middle">€{(v/1000).toFixed(1)}k</text>
            )}
            <text x={x + barW/2} y={H + 18} fontFamily={sx.sans.fontFamily} fontSize="11"
              fill={isCurrent ? Hearth.ink : Hearth.mute}
              fontWeight={isCurrent ? 700 : 500}
              textAnchor="middle">{labels[i]}</text>
          </g>
        );
      })}
    </svg>
  );
}

function Card({ children, padding = 28, style = {}, ...rest }) {
  return (
    <div style={{
      background: Hearth.card, border: `1px solid ${Hearth.border}`,
      borderRadius: 22, padding,
      boxShadow: '0 1px 0 rgba(0,0,0,.02), 0 12px 28px -22px rgba(60,30,10,.18)',
      ...style,
    }} {...rest}>{children}</div>
  );
}

function SectionLabel({ children, accent = Hearth.clayDeep }) {
  return (
    <div style={{ ...sx.sans, fontSize: 11, fontWeight: 700, letterSpacing: '0.14em', textTransform: 'uppercase', color: accent, marginBottom: 12 }}>{children}</div>
  );
}

// === Greeting strip — editorial moment ===
function GreetingStrip() {
  return (
    <div style={{ padding: '32px 32px 0' }}>
      <div style={{ display: 'flex', alignItems: 'flex-end', justifyContent: 'space-between', gap: 32 }}>
        <div>
          <div style={{ ...sx.sans, fontSize: 13, color: Hearth.mute, fontWeight: 600, letterSpacing: '0.04em', marginBottom: 10 }}>Tuesday, September 12 · 9:08 am</div>
          <div style={{ ...sx.serifDisplay, fontSize: 52, color: Hearth.ink, lineHeight: 1.0, letterSpacing: '-0.01em' }}>
            Morning, Maya — <em style={{ ...sx.serifDisplay, fontStyle: 'italic', color: Hearth.clay }}>September</em><br/>
            is looking softer than August.
          </div>
        </div>
        <div style={{ display: 'flex', gap: 10 }}>
          <button style={{ ...sx.sans, padding: '10px 16px', fontSize: 13, fontWeight: 600, background: Hearth.card, color: Hearth.ink, border: `1px solid ${Hearth.border}`, borderRadius: 999, cursor: 'pointer', display: 'inline-flex', alignItems: 'center', gap: 8 }}>
            <Icon.search size={14} /> Search expenses
          </button>
          <button style={{ ...sx.sans, padding: '10px 16px', fontSize: 13, fontWeight: 600, background: Hearth.card, color: Hearth.ink, border: `1px solid ${Hearth.border}`, borderRadius: 999, cursor: 'pointer', display: 'inline-flex', alignItems: 'center', gap: 8 }}>
            Sep 2026 ▾
          </button>
        </div>
      </div>
    </div>
  );
}

// === The big "this month" hero card ===
function MonthHero() {
  return (
    <Card style={{ padding: 32, gridColumn: 'span 8' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 24 }}>
        <div>
          <SectionLabel>This month · September</SectionLabel>
          <div style={{ display: 'flex', alignItems: 'baseline', gap: 14 }}>
            <div style={{ ...sx.serifDisplay, fontSize: 84, color: Hearth.ink, lineHeight: 0.95, letterSpacing: '-0.02em' }}>
              €2,430<span style={{ color: Hearth.mute, fontSize: 48 }}>.50</span>
            </div>
            <Pill bg={Hearth.sageSoft} color="#3F5C32" size="md"><Icon.arrowDown size={12} /> 8% vs Aug</Pill>
          </div>
          <div style={{ ...sx.sans, fontSize: 15, color: Hearth.ink2, marginTop: 8 }}>
            <strong style={{ color: Hearth.ink, fontWeight: 700 }}>€1,370</strong> left of <strong style={{ color: Hearth.ink, fontWeight: 700 }}>€3,800</strong> · 18 days to go
          </div>
        </div>

        {/* Streak / nudge */}
        <div style={{ width: 200, padding: 16, background: Hearth.mustardSoft, borderRadius: 16, ...sx.sans, fontSize: 13, color: '#5C4117' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 8 }}>
            <Icon.spark size={16} /> <strong style={{ ...sx.sans, fontWeight: 700 }}>5-week streak</strong>
          </div>
          You've come in under budget five weeks running. Keep it up and you'll close September with ~€470 to spare.
        </div>
      </div>

      <div style={{ marginTop: 10 }}>
        <SpendChart />
      </div>
    </Card>
  );
}

// === Category donut card ===
function CategoryDonut() {
  const segs = [
    { cat: Categories[0], v: 870, p: 36 },
    { cat: Categories[2], v: 530, p: 22 },
    { cat: Categories[1], v: 390, p: 16 },
    { cat: Categories[3], v: 290, p: 12 },
    { cat: Categories[4], v: 195, p:  8 },
    { cat: Categories[5], v: 156, p:  6 },
  ];
  const R = 88, C = 2 * Math.PI * R;
  let off = 0;
  return (
    <Card style={{ gridColumn: 'span 4' }}>
      <SectionLabel>Where it went</SectionLabel>
      <div style={{ display: 'flex', gap: 14, alignItems: 'center' }}>
        <div style={{ position: 'relative', width: 200, height: 200, flexShrink: 0 }}>
          <svg width={200} height={200} viewBox="0 0 200 200" style={{ transform: 'rotate(-90deg)' }}>
            <circle cx={100} cy={100} r={R} fill="none" stroke={Hearth.cardAlt} strokeWidth={22} />
            {segs.map((s, i) => {
              const len = (s.p/100)*C;
              const el = <circle key={i} cx={100} cy={100} r={R} fill="none" stroke={s.cat.color} strokeWidth={22} strokeDasharray={`${len} ${C - len}`} strokeDashoffset={-off} />;
              off += len + 3;
              return el;
            })}
          </svg>
          <div style={{ position: 'absolute', inset: 0, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center' }}>
            <div style={{ ...sx.sans, fontSize: 10, color: Hearth.mute, fontWeight: 700, letterSpacing: '0.1em' }}>BIGGEST</div>
            <div style={{ ...sx.serifDisplay, fontSize: 24, color: Hearth.ink, lineHeight: 1 }}>Groceries</div>
            <div style={{ ...sx.mono, fontSize: 13, color: Hearth.mute }}>€870</div>
          </div>
        </div>
        <div style={{ flex: 1, display: 'flex', flexDirection: 'column', gap: 8 }}>
          {segs.map(s => (
            <div key={s.cat.id} style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <span style={{ width: 8, height: 8, borderRadius: 2, background: s.cat.color }} />
              <span style={{ ...sx.sans, fontSize: 12, color: Hearth.ink2, flex: 1, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{s.cat.name}</span>
              <span style={{ ...sx.mono, fontSize: 12, color: Hearth.ink, fontWeight: 500 }}>€{s.v}</span>
            </div>
          ))}
        </div>
      </div>
    </Card>
  );
}

// === Recent expenses feed ===
const recentExpenses = [
  { who: 'Maya',  whoColor: Hearth.clay,    cat: Categories[0], merch: 'Mercadona', label: 'The weekly shop', when: 'Today · 09:14', amt: 80.90, split: 'Split 3' },
  { who: 'Jonah', whoColor: Hearth.sage,    cat: Categories[3], merch: 'Cabify',     label: 'Cab to the airport', when: 'Today · 06:22', amt: 24.50, split: null },
  { who: 'Iris',  whoColor: Hearth.mustard, cat: Categories[6], merch: 'Bookshop',   label: 'New Roald Dahl', when: 'Yesterday', amt: 12.40, split: 'Pocket' },
  { who: 'Maya',  whoColor: Hearth.clay,    cat: Categories[4], merch: 'Sushi&Co',   label: 'Tuesday takeaway', when: 'Yesterday · 19:48', amt: 28.50, split: 'Split 2' },
  { who: 'Maya',  whoColor: Hearth.clay,    cat: Categories[1], merch: 'Piano school', label: 'Iris · monthly tuition', when: 'Mon · 14:00', amt: 92.00, split: null, recurring: true },
  { who: 'Jonah', whoColor: Hearth.sage,    cat: Categories[7], merch: 'Endesa',     label: 'Electricity · August', when: 'Sun · 23:00', amt: 64.20, split: 'Split 2', recurring: true },
  { who: 'Maya',  whoColor: Hearth.clay,    cat: Categories[2], merch: 'IKEA',       label: 'Bedroom shelves', when: 'Sat', amt: 134.00, split: 'Split 2' },
];

function ExpenseRow({ e, last }) {
  return (
    <div style={{
      display: 'flex', alignItems: 'center', gap: 14, padding: '14px 0',
      borderBottom: last ? 'none' : `1px solid ${Hearth.divider}`,
    }}>
      <Avatar name={e.who} color={e.whoColor} size={36} />
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{ ...sx.sans, fontSize: 15, color: Hearth.ink, fontWeight: 600, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{e.label}</div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginTop: 2 }}>
          <Pill bg={e.cat.soft} color={e.cat.color}>{e.cat.glyph} {e.cat.name}</Pill>
          <span style={{ ...sx.sans, fontSize: 12, color: Hearth.mute }}>{e.merch} · {e.when}</span>
          {e.recurring && <Pill bg={Hearth.cardAlt} color={Hearth.mute}>↻ recurring</Pill>}
        </div>
      </div>
      <div style={{ textAlign: 'right' }}>
        <div style={{ ...sx.mono, fontSize: 16, color: Hearth.ink, fontWeight: 500 }}>€{e.amt.toFixed(2)}</div>
        {e.split && <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, marginTop: 2 }}>{e.split}</div>}
      </div>
    </div>
  );
}

function RecentExpenses() {
  return (
    <Card style={{ gridColumn: 'span 8' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
        <SectionLabel>Recently in the kitty</SectionLabel>
        <a style={{ ...sx.sans, fontSize: 13, color: Hearth.clayDeep, fontWeight: 600, textDecoration: 'none', display: 'inline-flex', alignItems: 'center', gap: 4 }}>See all 47 this month <Icon.arrowRight size={12} /></a>
      </div>
      <div>
        {recentExpenses.map((e,i) => <ExpenseRow key={i} e={e} last={i === recentExpenses.length - 1} />)}
      </div>
    </Card>
  );
}

// === Budgets list ===
function BudgetBar({ cat, spent, budget }) {
  const pct = Math.min(100, (spent / budget) * 100);
  const over = spent > budget;
  return (
    <div style={{ marginBottom: 14 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', marginBottom: 6 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <span style={{ fontSize: 16 }}>{cat.glyph}</span>
          <span style={{ ...sx.sans, fontSize: 14, color: Hearth.ink, fontWeight: 600 }}>{cat.name}</span>
        </div>
        <div style={{ ...sx.mono, fontSize: 13, color: over ? Hearth.berry : Hearth.ink2, fontWeight: 500 }}>
          €{spent} <span style={{ color: Hearth.mute, fontWeight: 400 }}>/ €{budget}</span>
        </div>
      </div>
      <div style={{ height: 8, borderRadius: 999, background: Hearth.cardAlt, overflow: 'hidden', position: 'relative' }}>
        <div style={{ width: `${pct}%`, height: '100%', background: over ? Hearth.berry : cat.color, borderRadius: 999 }} />
      </div>
    </div>
  );
}

function Budgets() {
  return (
    <Card style={{ gridColumn: 'span 4' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 14 }}>
        <SectionLabel>Budget vs actual</SectionLabel>
        <a style={{ ...sx.sans, fontSize: 12, color: Hearth.clayDeep, fontWeight: 600, textDecoration: 'none' }}>Edit</a>
      </div>
      <BudgetBar cat={Categories[0]} spent={870} budget={900} />
      <BudgetBar cat={Categories[2]} spent={530} budget={650} />
      <BudgetBar cat={Categories[1]} spent={390} budget={350} />
      <BudgetBar cat={Categories[3]} spent={290} budget={400} />
      <BudgetBar cat={Categories[4]} spent={195} budget={200} />

      <div style={{ marginTop: 16, padding: 14, background: Hearth.berrySoft, borderRadius: 14, display: 'flex', alignItems: 'flex-start', gap: 10 }}>
        <span style={{ width: 24, height: 24, borderRadius: '50%', background: Hearth.berry, color: '#fff', display: 'inline-flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0, ...sx.sans, fontSize: 13, fontWeight: 700 }}>!</span>
        <div style={{ ...sx.sans, fontSize: 13, color: '#7A2820', lineHeight: 1.4 }}>
          <strong>Kids & school</strong> ran €40 over — piano was paid early this month.
        </div>
      </div>
    </Card>
  );
}

// === Who paid what (members) ===
function MembersCard() {
  const members = [
    { name: 'Maya', color: Hearth.clay, spent: 1380, share: 0.57 },
    { name: 'Jonah', color: Hearth.sage, spent: 920, share: 0.38 },
    { name: 'Iris', color: Hearth.mustard, spent: 130, share: 0.05, note: 'pocket' },
  ];
  return (
    <Card style={{ gridColumn: 'span 4' }}>
      <SectionLabel>Who paid what</SectionLabel>
      {/* stacked bar */}
      <div style={{ display: 'flex', height: 14, borderRadius: 999, overflow: 'hidden', marginBottom: 16 }}>
        {members.map(m => (
          <div key={m.name} style={{ width: `${m.share * 100}%`, background: m.color }} />
        ))}
      </div>
      {members.map((m, i) => (
        <div key={m.name} style={{ display: 'flex', alignItems: 'center', gap: 12, padding: '10px 0', borderTop: i ? `1px solid ${Hearth.divider}` : 'none' }}>
          <Avatar name={m.name} color={m.color} size={32} />
          <div style={{ flex: 1 }}>
            <div style={{ ...sx.sans, fontSize: 14, color: Hearth.ink, fontWeight: 600 }}>{m.name}</div>
            <div style={{ ...sx.sans, fontSize: 12, color: Hearth.mute }}>{Math.round(m.share*100)}% of the kitty {m.note ? `· ${m.note}` : ''}</div>
          </div>
          <div style={{ ...sx.mono, fontSize: 15, color: Hearth.ink, fontWeight: 500 }}>€{m.spent}</div>
        </div>
      ))}
      <div style={{ marginTop: 14, padding: 12, background: Hearth.claySofter, borderRadius: 12, ...sx.sans, fontSize: 13, color: Hearth.clayDeep, display: 'flex', alignItems: 'center', gap: 10 }}>
        <span style={{ width: 24, height: 24, borderRadius: '50%', background: Hearth.clay, color: '#fff', display: 'inline-flex', alignItems: 'center', justifyContent: 'center' }}>↺</span>
        Jonah owes Maya <strong style={{ ...sx.mono, fontWeight: 600 }}>€186.40</strong>
        <button style={{ marginLeft: 'auto', ...sx.sans, fontSize: 12, fontWeight: 700, background: Hearth.clay, color: '#fff', border: 'none', borderRadius: 999, padding: '6px 12px', cursor: 'pointer' }}>Settle</button>
      </div>
    </Card>
  );
}

// === Upcoming bills ===
function Upcoming() {
  const bills = [
    { day: '15', mo: 'SEP', cat: Categories[7], label: 'Rent · Calle Velázquez', amt: 1450, soon: true },
    { day: '18', mo: 'SEP', cat: Categories[1], label: 'Iris — swimming Q4', amt: 165 },
    { day: '22', mo: 'SEP', cat: Categories[5], label: 'Maya — dentist', amt: 80 },
    { day: '28', mo: 'SEP', cat: Categories[7], label: 'Internet — Movistar', amt: 39.90 },
  ];
  return (
    <Card style={{ gridColumn: 'span 8' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 14 }}>
        <SectionLabel>Heads-up · the next two weeks</SectionLabel>
        <span style={{ ...sx.sans, fontSize: 13, color: Hearth.mute }}>4 recurring · €1,734.90 total</span>
      </div>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 14 }}>
        {bills.map((b, i) => (
          <div key={i} style={{
            padding: 18, background: b.soon ? Hearth.claySofter : Hearth.paper,
            borderRadius: 16, border: `1px solid ${b.soon ? Hearth.claySoft : Hearth.border}`,
          }}>
            <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', marginBottom: 10 }}>
              <div>
                <div style={{ ...sx.serifDisplay, fontSize: 32, color: Hearth.ink, lineHeight: 1 }}>{b.day}</div>
                <div style={{ ...sx.sans, fontSize: 10, color: Hearth.mute, fontWeight: 700, letterSpacing: '0.1em' }}>{b.mo}</div>
              </div>
              {b.soon && <Pill bg={Hearth.clay} color="#fff">in 3 days</Pill>}
            </div>
            <Pill bg={b.cat.soft} color={b.cat.color}>{b.cat.glyph} {b.cat.name}</Pill>
            <div style={{ ...sx.sans, fontSize: 13, color: Hearth.ink, fontWeight: 600, marginTop: 10, lineHeight: 1.3 }}>{b.label}</div>
            <div style={{ ...sx.mono, fontSize: 18, color: Hearth.ink, marginTop: 4 }}>€{b.amt.toFixed(2)}</div>
          </div>
        ))}
      </div>
    </Card>
  );
}

// === Currency / travel ===
function Currencies() {
  const cur = [
    { sym: '€', name: 'Euro',    v: 2430.50, primary: true },
    { sym: '£', name: 'Pound',   v:  184.00, note: 'London weekend' },
    { sym: '$', name: 'Dollar',  v:   62.50, note: 'Maya · iCloud' },
  ];
  return (
    <Card style={{ gridColumn: 'span 4' }}>
      <SectionLabel>Across currencies</SectionLabel>
      {cur.map((c, i) => (
        <div key={c.sym} style={{ display: 'flex', alignItems: 'center', gap: 12, padding: '10px 0', borderTop: i ? `1px solid ${Hearth.divider}` : 'none' }}>
          <span style={{
            width: 36, height: 36, borderRadius: 10, background: c.primary ? Hearth.claySofter : Hearth.cardAlt,
            color: c.primary ? Hearth.clayDeep : Hearth.ink, display: 'flex', alignItems: 'center', justifyContent: 'center',
            ...sx.serifDisplay, fontSize: 22,
          }}>{c.sym}</span>
          <div style={{ flex: 1 }}>
            <div style={{ ...sx.sans, fontSize: 14, color: Hearth.ink, fontWeight: 600 }}>{c.name}</div>
            <div style={{ ...sx.sans, fontSize: 12, color: Hearth.mute }}>{c.primary ? 'Household primary' : c.note}</div>
          </div>
          <div style={{ ...sx.mono, fontSize: 15, color: Hearth.ink, fontWeight: 500 }}>{c.sym}{c.v.toFixed(2)}</div>
        </div>
      ))}
      <div style={{ marginTop: 14, ...sx.sans, fontSize: 12, color: Hearth.mute }}>
        Converted at today's rate · €246.60 in non-€ this month
      </div>
    </Card>
  );
}

// === The full dashboard ===
function DashboardA() {
  return (
    <div style={{ background: Hearth.paper, color: Hearth.ink, ...sx.sans }}>
      <AppNav />
      <GreetingStrip />
      <div style={{ padding: '24px 32px 40px', display: 'grid', gridTemplateColumns: 'repeat(12, 1fr)', gap: 20 }}>
        <MonthHero />
        <CategoryDonut />
        <RecentExpenses />
        <Budgets />
        <Upcoming />
        <MembersCard />
        <Currencies />
      </div>
    </div>
  );
}

window.DashboardA = DashboardA;
window.AppNav = AppNav;
window.SpendChart = SpendChart;
window.Card = Card;
window.SectionLabel = SectionLabel;
window.ExpenseRow = ExpenseRow;
window.recentExpenses = recentExpenses;
