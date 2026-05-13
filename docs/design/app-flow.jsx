// In-app screens: expenses list, quick-add modal, family management.

// ─── Expenses list ────────────────────────────────────────────────
function ExpensesArtboard() {
  // 21 rows of expenses
  const rows = [
    ['Today',  '09:14', Categories[0], 'The weekly shop',  'Mercadona',   'Maya',  Hearth.clay,    80.90, 'Split 3'],
    ['Today',  '06:22', Categories[3], 'Cab to airport',   'Cabify',      'Jonah', Hearth.sage,    24.50, null],
    ['Yest.',  '20:11', Categories[6], 'New Roald Dahl',   'Casa del Libro', 'Iris',  Hearth.mustard, 12.40, 'Pocket'],
    ['Yest.',  '19:48', Categories[4], 'Tuesday takeaway', 'Sushi & Co',  'Maya',  Hearth.clay,    28.50, 'Split 2'],
    ['Yest.',  '16:30', Categories[1], 'Piano · Sep tuition','Pianoforte', 'Maya',  Hearth.clay,    92.00, null],
    ['Mon',    '14:00', Categories[7], 'Electricity Aug',  'Endesa',      'Jonah', Hearth.sage,    64.20, 'Split 2'],
    ['Sun',    '23:00', Categories[2], 'Bedroom shelves',  'IKEA',        'Maya',  Hearth.clay,   134.00, 'Split 2'],
    ['Sun',    '12:00', Categories[0], 'Brunch · sourdough','Panic Bakery','Jonah', Hearth.sage,    18.40, 'Split 2'],
    ['Sat',    '21:14', Categories[4], 'Movie + popcorn',  'Cinesa',      'Maya',  Hearth.clay,    32.00, 'Split 3'],
    ['Sat',    '11:02', Categories[0], 'Big weekly shop',  'Mercadona',   'Maya',  Hearth.clay,   140.10, 'Split 3'],
    ['Fri',    '20:30', Categories[6], 'Family game night','Toys R Us',   'Jonah', Hearth.sage,    24.80, null],
    ['Fri',    '08:14', Categories[3], 'Petrol — full tank','Repsol',     'Jonah', Hearth.sage,    62.00, 'Split 2'],
    ['Thu',    '17:00', Categories[5], 'Pharmacy',          'Farmacia',  'Maya',  Hearth.clay,    14.20, null],
    ['Thu',    '13:15', Categories[1], 'Iris school lunch', 'Colegio',   'Maya',  Hearth.clay,    18.00, null],
    ['Wed',    '19:00', Categories[6], 'Iris piano book',   'Pianoforte','Maya',  Hearth.clay,    22.50, null],
    ['Wed',    '08:00', Categories[3], 'Metro · 10-pack',   'EMT',       'Maya',  Hearth.clay,    12.20, null],
    ['Tue',    '22:30', Categories[4], 'Date night',        'Botín',     'Jonah', Hearth.sage,    78.00, 'Split 2'],
    ['Tue',    '12:00', Categories[2], 'New rug, hallway',  'Zara Home', 'Maya',  Hearth.clay,    65.00, 'Split 2'],
    ['Mon',    '09:14', Categories[0], 'Veg market',        'Mercado Maravillas','Maya', Hearth.clay, 18.80, 'Split 3'],
    ['1 Sep',  '11:11', Categories[1], 'Back to school haul','El Corte Inglés', 'Maya', Hearth.clay,   184.20, 'Split 2'],
    ['1 Sep',  '10:00', Categories[7], 'Rent · September',  'Landlord',   'Maya',  Hearth.clay,  1450.00, 'Split 2'],
  ];

  const headerCell = { ...sx.sans, fontSize: 11, fontWeight: 700, letterSpacing: '0.14em', color: Hearth.mute, textTransform: 'uppercase', padding: '12px 14px', borderBottom: `1px solid ${Hearth.border}`, textAlign: 'left' };
  const dataCell = { ...sx.sans, fontSize: 14, color: Hearth.ink, padding: '14px 14px', borderBottom: `1px solid ${Hearth.divider}` };

  return (
    <div style={{ background: Hearth.paper, color: Hearth.ink, ...sx.sans }}>
      <AppNav />

      {/* Title + filter row */}
      <div style={{ padding: '32px 32px 16px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end', marginBottom: 22 }}>
          <div>
            <div style={{ ...sx.sans, fontSize: 13, color: Hearth.mute, fontWeight: 600, marginBottom: 8 }}>Expenses · September 2026</div>
            <div style={{ ...sx.serifDisplay, fontSize: 48, color: Hearth.ink, lineHeight: 1, letterSpacing: '-0.01em' }}>
              47 entries this month
            </div>
          </div>
          <button style={{
            ...sx.sans, fontWeight: 600, fontSize: 14, padding: '12px 18px',
            background: Hearth.clay, color: '#fff', border: 'none', borderRadius: 999, cursor: 'pointer',
            display: 'inline-flex', alignItems: 'center', gap: 8,
            boxShadow: `0 8px 20px -8px ${Hearth.clay}aa`,
          }}><Icon.plus size={14} /> New expense</button>
        </div>

        {/* Filter chips */}
        <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap', alignItems: 'center', marginBottom: 14 }}>
          <div style={{ display: 'inline-flex', alignItems: 'center', gap: 8, padding: '8px 12px', background: Hearth.card, border: `1px solid ${Hearth.border}`, borderRadius: 999, ...sx.sans, fontSize: 13, color: Hearth.ink }}>
            <Icon.search size={13} /> <span style={{ color: Hearth.mute }}>Search expenses…</span>
          </div>
          <Pill bg={Hearth.ink} color={Hearth.paper} size="md">All categories ▾</Pill>
          <Pill bg={Hearth.card} color={Hearth.ink} size="md" style={{ border: `1px solid ${Hearth.border}` }}>Anyone ▾</Pill>
          <Pill bg={Hearth.card} color={Hearth.ink} size="md" style={{ border: `1px solid ${Hearth.border}` }}>This month ▾</Pill>
          <Pill bg={Hearth.claySofter} color={Hearth.clayDeep} size="md">{'>'} €50 ✕</Pill>
          <div style={{ marginLeft: 'auto', ...sx.sans, fontSize: 13, color: Hearth.mute }}>Showing 21 of 47</div>
        </div>
      </div>

      {/* Table */}
      <div style={{ padding: '0 32px 40px' }}>
        <div style={{ background: Hearth.card, borderRadius: 20, border: `1px solid ${Hearth.border}`, overflow: 'hidden' }}>
          <div style={{ display: 'grid', gridTemplateColumns: '90px 1fr 220px 140px 140px 40px', background: Hearth.paper }}>
            <div style={headerCell}>When</div>
            <div style={headerCell}>Expense</div>
            <div style={headerCell}>Category</div>
            <div style={headerCell}>Paid by</div>
            <div style={{ ...headerCell, textAlign: 'right' }}>Amount</div>
            <div style={headerCell}></div>
          </div>
          {rows.map((r, i) => {
            const [day, time, cat, label, merch, who, whoColor, amt, split] = r;
            return (
              <div key={i} style={{ display: 'grid', gridTemplateColumns: '90px 1fr 220px 140px 140px 40px', background: i % 2 ? Hearth.paper : Hearth.card, alignItems: 'center' }}>
                <div style={{ ...dataCell, ...sx.sans, color: Hearth.mute }}>
                  <div style={{ fontWeight: 700, color: Hearth.ink }}>{day}</div>
                  <div style={{ fontSize: 11 }}>{time}</div>
                </div>
                <div style={dataCell}>
                  <div style={{ fontWeight: 600, color: Hearth.ink }}>{label}</div>
                  <div style={{ fontSize: 12, color: Hearth.mute }}>{merch}</div>
                </div>
                <div style={dataCell}>
                  <Pill bg={cat.soft} color={cat.color}>{cat.glyph} {cat.name}</Pill>
                </div>
                <div style={{ ...dataCell, display: 'flex', alignItems: 'center', gap: 8 }}>
                  <Avatar name={who} color={whoColor} size={24} />
                  <span>{who}</span>
                </div>
                <div style={{ ...dataCell, textAlign: 'right' }}>
                  <div style={{ ...sx.mono, fontSize: 15, fontWeight: 500, color: Hearth.ink }}>€{amt.toFixed(2)}</div>
                  {split && <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute }}>{split}</div>}
                </div>
                <div style={{ ...dataCell, textAlign: 'center', color: Hearth.mute, cursor: 'pointer' }}>
                  <Icon.more size={16} />
                </div>
              </div>
            );
          })}
        </div>
        {/* Footer / pagination + totals */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 18, ...sx.sans, fontSize: 13, color: Hearth.ink2 }}>
          <span>Page 1 of 3 · 21 entries shown</span>
          <span style={{ ...sx.sans, fontSize: 14 }}>Sum of selected:&nbsp;
            <strong style={{ ...sx.mono, color: Hearth.ink, fontSize: 16 }}>€2,597.30</strong>
          </span>
        </div>
      </div>
    </div>
  );
}

// ─── Quick-add expense modal ─────────────────────────────────────
function QuickAddArtboard() {
  return (
    <div style={{
      background: 'rgba(35, 23, 14, 0.4)',
      backdropFilter: 'blur(4px)',
      width: '100%', height: '100%', minHeight: 760,
      display: 'flex', alignItems: 'flex-start', justifyContent: 'center',
      padding: '40px 40px',
      boxSizing: 'border-box',
    }}>
      <div style={{
        background: Hearth.card, borderRadius: 24, width: '100%', maxWidth: 480,
        boxShadow: '0 40px 80px -20px rgba(35,23,14,.5), 0 0 0 1px rgba(35,23,14,.05)',
        overflow: 'hidden',
      }}>
        <div style={{ padding: '20px 24px', borderBottom: `1px solid ${Hearth.border}`, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div style={{ ...sx.serifDisplay, fontSize: 22, color: Hearth.ink }}>Add an expense</div>
          <span style={{ width: 28, height: 28, borderRadius: '50%', background: Hearth.cardAlt, color: Hearth.ink, display: 'inline-flex', alignItems: 'center', justifyContent: 'center', cursor: 'pointer', ...sx.sans, fontSize: 16 }}>×</span>
        </div>

        <div style={{ padding: '24px' }}>
          {/* Amount with currency picker */}
          <div style={{ display: 'flex', alignItems: 'baseline', gap: 12, padding: '6px 0 18px', borderBottom: `1px dashed ${Hearth.border}` }}>
            <span style={{ ...sx.serifDisplay, fontSize: 56, color: Hearth.mute }}>€</span>
            <input style={{
              ...sx.serifDisplay, fontSize: 64, color: Hearth.ink, background: 'transparent',
              border: 'none', outline: 'none', flex: 1, padding: 0, letterSpacing: '-0.01em',
            }} defaultValue="80.90" />
            <button style={{ ...sx.sans, fontSize: 12, color: Hearth.mute, background: Hearth.cardAlt, border: 'none', borderRadius: 999, padding: '6px 10px', cursor: 'pointer', fontWeight: 600 }}>EUR ▾</button>
          </div>

          {/* What */}
          <div style={{ marginTop: 18 }}>
            <label style={{ ...sx.sans, fontSize: 12, fontWeight: 700, color: Hearth.mute, letterSpacing: '0.1em' }}>WHAT WAS IT FOR?</label>
            <input style={{
              ...sx.sans, width: '100%', marginTop: 6, padding: '12px 14px', boxSizing: 'border-box',
              background: Hearth.paper, border: `1px solid ${Hearth.border}`, borderRadius: 12,
              fontSize: 16, color: Hearth.ink, outline: 'none',
            }} defaultValue="The weekly shop" />
          </div>

          {/* Category picker */}
          <div style={{ marginTop: 18 }}>
            <label style={{ ...sx.sans, fontSize: 12, fontWeight: 700, color: Hearth.mute, letterSpacing: '0.1em' }}>CATEGORY</label>
            <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap', marginTop: 8 }}>
              {Categories.slice(0,8).map((c, i) => (
                <span key={c.id} style={{
                  display: 'inline-flex', alignItems: 'center', gap: 6,
                  padding: '8px 12px', borderRadius: 10,
                  background: i === 0 ? c.color : c.soft,
                  color: i === 0 ? '#fff' : c.color,
                  ...sx.sans, fontSize: 13, fontWeight: 600, cursor: 'pointer',
                  border: i === 0 ? 'none' : `1px solid ${c.soft}`,
                }}>{c.glyph} {c.name}</span>
              ))}
            </div>
          </div>

          {/* Who paid */}
          <div style={{ marginTop: 18 }}>
            <label style={{ ...sx.sans, fontSize: 12, fontWeight: 700, color: Hearth.mute, letterSpacing: '0.1em' }}>WHO PAID?</label>
            <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
              {[
                { name: 'Maya',  color: Hearth.clay,    selected: true },
                { name: 'Jonah', color: Hearth.sage },
                { name: 'Iris',  color: Hearth.mustard },
                { name: 'Both adults', color: Hearth.ink },
              ].map(m => (
                <span key={m.name} style={{
                  display: 'inline-flex', alignItems: 'center', gap: 8,
                  padding: '8px 12px', borderRadius: 999,
                  background: m.selected ? m.color : Hearth.paper,
                  border: `1px solid ${m.selected ? m.color : Hearth.border}`,
                  color: m.selected ? '#fff' : Hearth.ink,
                  ...sx.sans, fontSize: 13, fontWeight: 600, cursor: 'pointer',
                }}>
                  {m.name !== 'Both adults' && <Avatar name={m.name} color={m.selected ? '#fff' : m.color} size={20} />}
                  <span style={{ color: m.selected ? '#fff' : (m.name === 'Both adults' ? Hearth.ink : 'inherit') }}>{m.name}</span>
                </span>
              ))}
            </div>
          </div>

          {/* Split */}
          <div style={{ marginTop: 18 }}>
            <label style={{ ...sx.sans, fontSize: 12, fontWeight: 700, color: Hearth.mute, letterSpacing: '0.1em' }}>SPLIT</label>
            <div style={{ marginTop: 8, padding: 14, background: Hearth.claySofter, borderRadius: 14 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 10, ...sx.sans, fontSize: 13, fontWeight: 600, color: Hearth.clayDeep }}>
                <span style={{ width: 14, height: 14, borderRadius: 4, background: Hearth.clay, color: '#fff', display: 'inline-flex', alignItems: 'center', justifyContent: 'center' }}><Icon.check size={9} /></span>
                Split evenly — 3 ways
              </div>
              <div style={{ display: 'flex', gap: 6 }}>
                {[
                  { who: 'Maya', color: Hearth.clay, amt: 26.97 },
                  { who: 'Jonah', color: Hearth.sage, amt: 26.97 },
                  { who: 'Iris', color: Hearth.mustard, amt: 26.96 },
                ].map(s => (
                  <div key={s.who} style={{ flex: 1, background: Hearth.card, borderRadius: 10, padding: '8px 10px', textAlign: 'center' }}>
                    <Avatar name={s.who} color={s.color} size={22} />
                    <div style={{ ...sx.mono, fontSize: 14, color: Hearth.ink, fontWeight: 500, marginTop: 4 }}>€{s.amt.toFixed(2)}</div>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* When + note row */}
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 10, marginTop: 16 }}>
            <div>
              <label style={{ ...sx.sans, fontSize: 11, fontWeight: 700, color: Hearth.mute, letterSpacing: '0.1em' }}>WHEN</label>
              <div style={{ marginTop: 6, padding: '11px 12px', background: Hearth.paper, border: `1px solid ${Hearth.border}`, borderRadius: 10, ...sx.sans, fontSize: 14, color: Hearth.ink }}>Today, 09:14</div>
            </div>
            <div>
              <label style={{ ...sx.sans, fontSize: 11, fontWeight: 700, color: Hearth.mute, letterSpacing: '0.1em' }}>NOTE</label>
              <div style={{ marginTop: 6, padding: '11px 12px', background: Hearth.paper, border: `1px solid ${Hearth.border}`, borderRadius: 10, ...sx.sans, fontSize: 13, color: Hearth.mute, fontStyle: 'italic' }}>(optional)</div>
            </div>
          </div>
        </div>

        <div style={{ padding: '18px 24px', borderTop: `1px solid ${Hearth.border}`, display: 'flex', gap: 10, background: Hearth.paper }}>
          <button style={{ ...sx.sans, fontSize: 14, fontWeight: 600, padding: '12px 16px', flex: 1, background: 'transparent', border: `1px solid ${Hearth.border}`, color: Hearth.ink, borderRadius: 12, cursor: 'pointer' }}>Save & add another</button>
          <button style={{ ...sx.sans, fontSize: 14, fontWeight: 600, padding: '12px 16px', flex: 1, background: Hearth.ink, color: Hearth.paper, border: 'none', borderRadius: 12, cursor: 'pointer', display: 'inline-flex', alignItems: 'center', justifyContent: 'center', gap: 6 }}>Save expense <Icon.check size={14} /></button>
        </div>
      </div>
    </div>
  );
}

// ─── Family management ─────────────────────────────────────────────
function FamilyArtboard() {
  const members = [
    { name: 'Maya Rivera',  role: 'Household head',  color: Hearth.clay,    age: null, joined: 'Mar 2025', sees: 'Everything' },
    { name: 'Jonah Rivera', role: 'Partner',         color: Hearth.sage,    age: null, joined: 'Mar 2025', sees: 'Everything except pocket money' },
    { name: 'Iris Rivera',  role: 'Kid · pocket holder', color: Hearth.mustard, age: 9,  joined: 'Sep 2025', sees: 'Just her pocket' },
    { name: 'Felix Rivera', role: 'Kid · pocket holder', color: Hearth.sky,     age: 12, joined: 'Sep 2025', sees: 'Just his pocket' },
  ];
  return (
    <div style={{ background: Hearth.paper, color: Hearth.ink, ...sx.sans }}>
      <AppNav />
      <div style={{ padding: '32px 32px 40px' }}>
        {/* Header */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end', marginBottom: 28 }}>
          <div>
            <div style={{ ...sx.sans, fontSize: 13, color: Hearth.mute, fontWeight: 600, marginBottom: 8 }}>Settings · Family</div>
            <div style={{ ...sx.serifDisplay, fontSize: 56, color: Hearth.ink, lineHeight: 1, letterSpacing: '-0.02em' }}>
              The Rivera household
            </div>
            <div style={{ ...sx.sans, fontSize: 15, color: Hearth.ink2, marginTop: 8, maxWidth: 500 }}>
              4 members · 2 grown-ups, 2 kids · sharing the same money since March 2025.
            </div>
          </div>
          <div style={{ display: 'flex', gap: 10 }}>
            <button style={{ ...sx.sans, fontSize: 14, fontWeight: 600, padding: '10px 16px', background: Hearth.card, color: Hearth.ink, border: `1px solid ${Hearth.border}`, borderRadius: 999, cursor: 'pointer' }}>Switch household</button>
            <button style={{ ...sx.sans, fontSize: 14, fontWeight: 600, padding: '10px 16px', background: Hearth.clay, color: '#fff', border: 'none', borderRadius: 999, cursor: 'pointer', display: 'inline-flex', alignItems: 'center', gap: 6 }}><Icon.plus size={14} /> Invite</button>
          </div>
        </div>

        {/* Card grid */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 18 }}>
          {members.map(m => (
            <div key={m.name} style={{
              background: Hearth.card, border: `1px solid ${Hearth.border}`, borderRadius: 22, padding: 24,
              boxShadow: '0 12px 28px -22px rgba(60,30,10,.18)',
              display: 'flex', alignItems: 'flex-start', gap: 18,
            }}>
              <Avatar name={m.name} color={m.color} size={64} ring />
              <div style={{ flex: 1 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <div>
                    <div style={{ ...sx.serifDisplay, fontSize: 26, color: Hearth.ink, lineHeight: 1.1 }}>{m.name}</div>
                    <div style={{ ...sx.sans, fontSize: 12, color: Hearth.mute, fontWeight: 700, letterSpacing: '0.1em', textTransform: 'uppercase', marginTop: 4 }}>{m.role}{m.age && ` · ${m.age}`}</div>
                  </div>
                  <span style={{ color: Hearth.mute, cursor: 'pointer' }}><Icon.more size={18} /></span>
                </div>
                <div style={{ marginTop: 14, display: 'flex', flexDirection: 'column', gap: 8 }}>
                  <div style={{ ...sx.sans, fontSize: 13, color: Hearth.ink2, display: 'flex', alignItems: 'center', gap: 6 }}>
                    <span style={{ color: Hearth.mute, fontSize: 11, fontWeight: 700, letterSpacing: '0.1em', minWidth: 56 }}>JOINED</span> {m.joined}
                  </div>
                  <div style={{ ...sx.sans, fontSize: 13, color: Hearth.ink2, display: 'flex', alignItems: 'center', gap: 6 }}>
                    <span style={{ color: Hearth.mute, fontSize: 11, fontWeight: 700, letterSpacing: '0.1em', minWidth: 56 }}>SEES</span> {m.sees}
                  </div>
                </div>
              </div>
            </div>
          ))}

          {/* Invite card */}
          <div style={{
            border: `1.5px dashed ${Hearth.claySoft}`, borderRadius: 22, padding: 24,
            display: 'flex', alignItems: 'center', justifyContent: 'center', flexDirection: 'column', gap: 10,
            background: 'transparent',
          }}>
            <span style={{ width: 48, height: 48, borderRadius: '50%', background: Hearth.claySofter, color: Hearth.clayDeep, display: 'inline-flex', alignItems: 'center', justifyContent: 'center' }}><Icon.plus size={22} /></span>
            <div style={{ ...sx.serifDisplay, fontSize: 22, color: Hearth.ink }}>Invite a grown-up or kid</div>
            <div style={{ ...sx.sans, fontSize: 13, color: Hearth.mute, textAlign: 'center', maxWidth: 280 }}>Send a link by email. They'll set their own password.</div>
          </div>
        </div>
      </div>
    </div>
  );
}

window.ExpensesArtboard = ExpensesArtboard;
window.QuickAddArtboard = QuickAddArtboard;
window.FamilyArtboard = FamilyArtboard;
