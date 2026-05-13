// Dashboard B — Soft Cards. Same Hearth palette, but the layout leans on
// big color-blocked tiles, more whitespace, friendlier illustration cues.
// More approachable, less "fintech".

function ColoredTile({ bg, fg = Hearth.ink, padding = 28, children, style = {} }) {
  return (
    <div style={{
      background: bg, color: fg, borderRadius: 28, padding,
      boxShadow: '0 1px 0 rgba(0,0,0,.02), 0 16px 32px -24px rgba(60,30,10,.15)',
      ...style,
    }}>{children}</div>
  );
}

// Bigger, blockier "month" hero with embedded stat tiles
function HeroBigB() {
  return (
    <ColoredTile bg={Hearth.claySofter} style={{ gridColumn: 'span 8', padding: 36 }}>
      <div style={{ ...sx.sans, fontSize: 12, fontWeight: 700, letterSpacing: '0.16em', color: Hearth.clayDeep }}>SEPTEMBER · WE'RE 18 DAYS IN</div>
      <div style={{ ...sx.serifDisplay, fontSize: 96, color: Hearth.ink, lineHeight: 0.95, marginTop: 14, letterSpacing: '-0.02em' }}>
        €2,430<span style={{ color: Hearth.clay }}>.50</span>
      </div>
      <div style={{ ...sx.sans, fontSize: 16, color: Hearth.ink2, marginTop: 8 }}>spent this month. <strong style={{ color: Hearth.clayDeep, fontWeight: 700 }}>€1,370 still in the jar.</strong></div>

      {/* Soft progress arc */}
      <div style={{ marginTop: 24, height: 12, borderRadius: 999, background: Hearth.card, position: 'relative', overflow: 'hidden' }}>
        <div style={{ width: '64%', height: '100%', background: `linear-gradient(90deg, ${Hearth.clay}, ${Hearth.mustard})`, borderRadius: 999 }} />
      </div>
      <div style={{ display: 'flex', justifyContent: 'space-between', ...sx.sans, fontSize: 12, color: Hearth.clayDeep, fontWeight: 600, marginTop: 8 }}>
        <span>64% of €3,800 spent</span>
        <span>Comfortable. Trend: −8%</span>
      </div>

      {/* 3 mini stats inline */}
      <div style={{ marginTop: 28, display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 14 }}>
        {[
          { label: 'Daily average', value: '€135', sub: 'last 30 days' },
          { label: 'Biggest day',  value: '€241', sub: 'Sat · weekly shop' },
          { label: 'No-spend days', value: '6', sub: 'this month' },
        ].map(s => (
          <div key={s.label} style={{ background: Hearth.card, borderRadius: 18, padding: 16 }}>
            <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, fontWeight: 700, letterSpacing: '0.1em' }}>{s.label.toUpperCase()}</div>
            <div style={{ ...sx.serifDisplay, fontSize: 32, color: Hearth.ink, marginTop: 2 }}>{s.value}</div>
            <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, marginTop: 2 }}>{s.sub}</div>
          </div>
        ))}
      </div>
    </ColoredTile>
  );
}

// "What's costing the most" — vertical category bars
function CategoryBlocksB() {
  const items = [
    { cat: Categories[0], v: 870 },
    { cat: Categories[2], v: 530 },
    { cat: Categories[1], v: 390 },
    { cat: Categories[3], v: 290 },
    { cat: Categories[4], v: 195 },
  ];
  const max = 900;
  return (
    <ColoredTile bg={Hearth.card} style={{ gridColumn: 'span 4', border: `1px solid ${Hearth.border}` }}>
      <div style={{ ...sx.sans, fontSize: 11, fontWeight: 700, letterSpacing: '0.14em', textTransform: 'uppercase', color: Hearth.mute, marginBottom: 16 }}>By category</div>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
        {items.map(it => (
          <div key={it.cat.id}>
            <div style={{ display: 'flex', justifyContent: 'space-between', ...sx.sans, fontSize: 13, color: Hearth.ink, fontWeight: 600, marginBottom: 6, alignItems: 'center' }}>
              <span style={{ display: 'inline-flex', alignItems: 'center', gap: 8 }}>
                <span style={{ fontSize: 16 }}>{it.cat.glyph}</span>
                {it.cat.name}
              </span>
              <span style={{ ...sx.mono, fontWeight: 500, color: Hearth.ink2 }}>€{it.v}</span>
            </div>
            <div style={{ height: 10, borderRadius: 999, background: it.cat.soft, overflow: 'hidden' }}>
              <div style={{ width: `${(it.v/max)*100}%`, height: '100%', background: it.cat.color, borderRadius: 999 }} />
            </div>
          </div>
        ))}
      </div>
    </ColoredTile>
  );
}

// Mini "feed" with member-colored chips
function ActivityFeedB() {
  return (
    <ColoredTile bg={Hearth.card} style={{ gridColumn: 'span 8', border: `1px solid ${Hearth.border}` }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 18 }}>
        <div style={{ ...sx.serifDisplay, fontSize: 28, color: Hearth.ink }}>This week in the household</div>
        <button style={{ ...sx.sans, fontSize: 13, color: Hearth.clayDeep, fontWeight: 600, background: 'transparent', border: 'none', cursor: 'pointer' }}>View all →</button>
      </div>
      {recentExpenses.slice(0, 5).map((e, i) => (
        <ExpenseRow key={i} e={e} last={i === 4} />
      ))}
    </ColoredTile>
  );
}

function PocketCardsB() {
  const kids = [
    { name: 'Iris', age: 9, color: Hearth.mustard, bg: Hearth.mustardSoft, deep: '#7A5C1F', save: 18.50, pct: 62, goal: 'A new scooter' },
    { name: 'Felix', age: 12, color: Hearth.sky, bg: Hearth.skySoft, deep: '#2E5663', save: 42.00, pct: 38, goal: 'Switch game' },
  ];
  return (
    <ColoredTile bg={Hearth.sageSoft} style={{ gridColumn: 'span 4', padding: 28 }}>
      <div style={{ ...sx.sans, fontSize: 12, fontWeight: 700, letterSpacing: '0.14em', color: '#3F5C32', marginBottom: 6 }}>POCKET MONEY</div>
      <div style={{ ...sx.serifDisplay, fontSize: 28, color: Hearth.ink, marginBottom: 16, lineHeight: 1.1 }}>The kids are saving.</div>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
        {kids.map(k => (
          <div key={k.name} style={{ background: Hearth.card, borderRadius: 16, padding: 14, display: 'flex', alignItems: 'center', gap: 14 }}>
            <Avatar name={k.name} color={k.color} size={42} />
            <div style={{ flex: 1, minWidth: 0 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline' }}>
                <div style={{ ...sx.sans, fontSize: 14, color: Hearth.ink, fontWeight: 700 }}>{k.name}</div>
                <div style={{ ...sx.serifDisplay, fontSize: 22, color: Hearth.ink }}>€{k.save.toFixed(2)}</div>
              </div>
              <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, marginBottom: 6 }}>Saving for <strong style={{ color: Hearth.ink2 }}>{k.goal}</strong></div>
              <div style={{ height: 6, borderRadius: 999, background: Hearth.cardAlt }}>
                <div style={{ width: `${k.pct}%`, height: '100%', borderRadius: 999, background: k.color }} />
              </div>
            </div>
          </div>
        ))}
      </div>
    </ColoredTile>
  );
}

function BillsB() {
  const bills = [
    { day: '15', cat: Categories[7], label: 'Rent · Velázquez', amt: 1450 },
    { day: '18', cat: Categories[1], label: 'Iris · swimming Q4', amt: 165 },
    { day: '22', cat: Categories[5], label: 'Maya · dentist', amt: 80 },
  ];
  return (
    <ColoredTile bg={Hearth.skySoft} style={{ gridColumn: 'span 4' }}>
      <div style={{ ...sx.sans, fontSize: 12, fontWeight: 700, letterSpacing: '0.14em', color: '#2E5663', marginBottom: 6 }}>HEADS-UP</div>
      <div style={{ ...sx.serifDisplay, fontSize: 28, color: Hearth.ink, marginBottom: 16, lineHeight: 1.1 }}>3 bills next week.</div>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
        {bills.map((b, i) => (
          <div key={i} style={{ background: Hearth.card, borderRadius: 14, padding: '12px 14px', display: 'flex', alignItems: 'center', gap: 12 }}>
            <div style={{ width: 38, textAlign: 'center', flexShrink: 0 }}>
              <div style={{ ...sx.serifDisplay, fontSize: 22, color: Hearth.ink, lineHeight: 1 }}>{b.day}</div>
              <div style={{ ...sx.sans, fontSize: 9, color: Hearth.mute, fontWeight: 700, letterSpacing: '0.1em' }}>SEP</div>
            </div>
            <div style={{ flex: 1, minWidth: 0 }}>
              <div style={{ ...sx.sans, fontSize: 13, color: Hearth.ink, fontWeight: 600, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{b.label}</div>
              <Pill bg={b.cat.soft} color={b.cat.color}>{b.cat.glyph} {b.cat.name}</Pill>
            </div>
            <div style={{ ...sx.mono, fontSize: 14, color: Hearth.ink, fontWeight: 500 }}>€{b.amt.toFixed(0)}</div>
          </div>
        ))}
      </div>
    </ColoredTile>
  );
}

function MembersB() {
  const members = [
    { name: 'Maya', color: Hearth.clay, spent: 1380, share: 57 },
    { name: 'Jonah', color: Hearth.sage, spent: 920, share: 38 },
    { name: 'Iris', color: Hearth.mustard, spent: 130, share: 5 },
  ];
  return (
    <ColoredTile bg={Hearth.mustardSoft} style={{ gridColumn: 'span 4' }}>
      <div style={{ ...sx.sans, fontSize: 12, fontWeight: 700, letterSpacing: '0.14em', color: '#7A5C1F', marginBottom: 6 }}>WHO PAID</div>
      <div style={{ ...sx.serifDisplay, fontSize: 28, color: Hearth.ink, marginBottom: 16, lineHeight: 1.1 }}>Maya's covering more this month.</div>

      {members.map((m, i) => (
        <div key={m.name} style={{ display: 'flex', alignItems: 'center', gap: 12, padding: '10px 0', borderTop: i ? `1px solid rgba(122,92,31,0.2)` : 'none' }}>
          <Avatar name={m.name} color={m.color} size={32} />
          <div style={{ flex: 1, ...sx.sans, fontSize: 14, color: Hearth.ink, fontWeight: 600 }}>{m.name}</div>
          <div style={{ ...sx.mono, fontSize: 14, color: Hearth.ink, fontWeight: 500 }}>€{m.spent}</div>
          <div style={{ ...sx.sans, fontSize: 11, color: '#7A5C1F', fontWeight: 700, minWidth: 32, textAlign: 'right' }}>{m.share}%</div>
        </div>
      ))}
      <button style={{
        marginTop: 14, width: '100%',
        ...sx.sans, fontWeight: 600, fontSize: 13, padding: '10px',
        background: Hearth.ink, color: Hearth.paper, border: 'none', borderRadius: 12, cursor: 'pointer',
      }}>Settle Jonah → Maya · €186.40</button>
    </ColoredTile>
  );
}

function HeaderB() {
  return (
    <div style={{ padding: '36px 32px 0' }}>
      <div style={{ ...sx.sans, fontSize: 13, color: Hearth.mute, fontWeight: 600, marginBottom: 12 }}>Tuesday, September 12</div>
      <div style={{ ...sx.serifDisplay, fontSize: 56, color: Hearth.ink, letterSpacing: '-0.02em', lineHeight: 1, maxWidth: 800 }}>
        Hey Maya. The house, in big tiles.
      </div>
    </div>
  );
}

function DashboardB() {
  return (
    <div style={{ background: Hearth.paper, color: Hearth.ink, ...sx.sans }}>
      <AppNav />
      <HeaderB />
      <div style={{ padding: '28px 32px 40px', display: 'grid', gridTemplateColumns: 'repeat(12, 1fr)', gap: 18 }}>
        <HeroBigB />
        <CategoryBlocksB />
        <ActivityFeedB />
        <PocketCardsB />
        <BillsB />
        <MembersB />
      </div>
    </div>
  );
}

window.DashboardB = DashboardB;
