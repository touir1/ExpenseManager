// Dashboard C — Editorial. Newspaper-style hi-fi: tall columns, big serif
// headlines, monochrome, single clay accent. Lots of rules and small caps.

function RuleH({ label, side = 'left' }) {
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 16 }}>
      <div style={{ ...sx.sans, fontSize: 10, fontWeight: 700, letterSpacing: '0.2em', textTransform: 'uppercase', color: Hearth.mute, flexShrink: 0 }}>{label}</div>
      <div style={{ flex: 1, height: 1, background: Hearth.border }} />
    </div>
  );
}

function MastheadC() {
  return (
    <div style={{ padding: '36px 48px 24px', borderBottom: `2px solid ${Hearth.ink}` }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end' }}>
        <div>
          <div style={{ ...sx.sans, fontSize: 11, fontWeight: 700, letterSpacing: '0.22em', color: Hearth.mute }}>THE RIVERA HOUSEHOLD · VOL. III · NO. 9 · SEPTEMBER MMXXVI</div>
          <div style={{ ...sx.serifDisplay, fontSize: 72, color: Hearth.ink, marginTop: 6, letterSpacing: '-0.02em', lineHeight: 1 }}>
            The Daily <em style={{ ...sx.serifDisplay, fontStyle: 'italic', color: Hearth.clay }}>Kitty</em>
          </div>
        </div>
        <div style={{ ...sx.sans, fontSize: 12, color: Hearth.mute, textAlign: 'right', lineHeight: 1.7 }}>
          Tuesday, 12 Sep · Madrid<br/>
          18°C · sunny · €/$ 1.07<br/>
          5-week budget streak
        </div>
      </div>
    </div>
  );
}

// Lead story — month total
function LeadStoryC() {
  return (
    <div style={{ paddingRight: 32, borderRight: `1px solid ${Hearth.border}` }}>
      <div style={{ ...sx.sans, fontSize: 11, fontWeight: 700, letterSpacing: '0.18em', color: Hearth.clayDeep, marginBottom: 10 }}>SEPTEMBER · LEAD</div>
      <div style={{ ...sx.serifDisplay, fontSize: 64, lineHeight: 0.98, color: Hearth.ink, letterSpacing: '-0.02em', marginBottom: 12 }}>
        A quieter month
        <br/>
        than August, by
        <br/>
        <em style={{ ...sx.serifDisplay, fontStyle: 'italic', color: Hearth.clay }}>two hundred</em>
        <br/>and ten euros.
      </div>
      <div style={{ ...sx.sans, fontSize: 15, color: Hearth.ink2, lineHeight: 1.6, columnCount: 2, columnGap: 22, marginBottom: 24 }}>
        The household spent <strong style={{ color: Hearth.ink }}>€2,430.50</strong> in the first 18 days of September, a notable <strong style={{ color: Hearth.sage }}>−8% on August</strong>. Groceries lead the table at €870, followed by Home at €530. The weekly shop on Saturday was the single biggest day, ringing in at €241.
        Forecasts based on the current pace project a month-end total of roughly <strong style={{ color: Hearth.ink }}>€3,330</strong> — comfortably under the household's €3,800 budget, and the fifth consecutive month under target.
      </div>

      {/* Big number bar */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', borderTop: `1px solid ${Hearth.ink}`, borderBottom: `1px solid ${Hearth.ink}` }}>
        {[
          { label: 'Spent', value: '€2,430', sub: '−8% vs Aug' },
          { label: 'Left',  value: '€1,370', sub: 'in the kitty' },
          { label: 'Pace',  value: '€3,330', sub: 'projected close' },
          { label: 'Days',  value: '18', sub: 'into the month' },
        ].map((s, i) => (
          <div key={s.label} style={{ padding: '20px 18px', borderRight: i < 3 ? `1px solid ${Hearth.border}` : 'none' }}>
            <div style={{ ...sx.sans, fontSize: 10, color: Hearth.mute, fontWeight: 700, letterSpacing: '0.16em' }}>{s.label.toUpperCase()}</div>
            <div style={{ ...sx.serifDisplay, fontSize: 36, color: Hearth.ink, marginTop: 4, lineHeight: 1 }}>{s.value}</div>
            <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, marginTop: 4 }}>{s.sub}</div>
          </div>
        ))}
      </div>

      {/* Spend chart inset */}
      <div style={{ marginTop: 24 }}>
        <RuleH label="MONTHLY · NINE-MONTH RUN" />
        <SpendChart accent={Hearth.ink} soft={Hearth.cardAlt} />
      </div>
    </div>
  );
}

// Right column — categories + budget
function CategoriesColC() {
  const items = [
    { cat: Categories[0], v: 870, b: 900 },
    { cat: Categories[2], v: 530, b: 650 },
    { cat: Categories[1], v: 390, b: 350 },
    { cat: Categories[3], v: 290, b: 400 },
    { cat: Categories[4], v: 195, b: 200 },
    { cat: Categories[7], v: 156, b: 200 },
  ];
  return (
    <div>
      <div style={{ ...sx.sans, fontSize: 11, fontWeight: 700, letterSpacing: '0.18em', color: Hearth.clayDeep, marginBottom: 10 }}>BY THE CATEGORY</div>
      <div style={{ ...sx.serifDisplay, fontSize: 36, color: Hearth.ink, lineHeight: 1, marginBottom: 18, letterSpacing: '-0.01em' }}>
        What the<br/>money <em style={{ ...sx.serifDisplay, fontStyle: 'italic', color: Hearth.clay }}>bought.</em>
      </div>
      <div style={{ borderTop: `1px solid ${Hearth.ink}` }}>
        {items.map(it => {
          const pct = (it.v/it.b)*100;
          const over = it.v > it.b;
          return (
            <div key={it.cat.id} style={{ padding: '14px 0', borderBottom: `1px solid ${Hearth.border}` }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', marginBottom: 6 }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                  <span style={{ fontSize: 14 }}>{it.cat.glyph}</span>
                  <span style={{ ...sx.serifDisplay, fontSize: 20, color: Hearth.ink }}>{it.cat.name}</span>
                </div>
                <div style={{ ...sx.mono, fontSize: 14, color: over ? Hearth.berry : Hearth.ink }}>
                  €{it.v} <span style={{ color: Hearth.mute }}>/ €{it.b}</span>
                </div>
              </div>
              <div style={{ height: 4, background: Hearth.cardAlt, borderRadius: 99, overflow: 'hidden' }}>
                <div style={{ width: `${Math.min(100, pct)}%`, height: '100%', background: over ? Hearth.berry : Hearth.ink }} />
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

// Members column — "the contributors"
function MembersColC() {
  const m = [
    { name: 'Maya Rivera', role: 'Household head', color: Hearth.clay, spent: 1380, share: 57, biggest: 'Groceries · €620' },
    { name: 'Jonah Rivera', role: 'Partner', color: Hearth.sage, spent: 920, share: 38, biggest: 'Transport · €240' },
    { name: 'Iris Rivera', role: 'Pocket holder', color: Hearth.mustard, spent: 130, share: 5, biggest: 'Books · €40' },
  ];
  return (
    <div>
      <div style={{ ...sx.sans, fontSize: 11, fontWeight: 700, letterSpacing: '0.18em', color: Hearth.clayDeep, marginBottom: 10 }}>THE CONTRIBUTORS</div>
      <div style={{ ...sx.serifDisplay, fontSize: 36, color: Hearth.ink, lineHeight: 1, marginBottom: 18, letterSpacing: '-0.01em' }}>
        Who paid<br/>for <em style={{ ...sx.serifDisplay, fontStyle: 'italic', color: Hearth.clay }}>September.</em>
      </div>
      {m.map((p, i) => (
        <div key={p.name} style={{ display: 'flex', gap: 12, padding: '18px 0', borderTop: `1px solid ${Hearth.ink}`, alignItems: 'flex-start' }}>
          <Avatar name={p.name} color={p.color} size={44} />
          <div style={{ flex: 1 }}>
            <div style={{ ...sx.serifDisplay, fontSize: 20, color: Hearth.ink, lineHeight: 1.1 }}>{p.name}</div>
            <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, fontWeight: 700, letterSpacing: '0.12em', textTransform: 'uppercase', marginTop: 2 }}>{p.role}</div>
            <div style={{ ...sx.sans, fontSize: 13, color: Hearth.ink2, marginTop: 8 }}>
              Biggest line: {p.biggest}
            </div>
          </div>
          <div style={{ textAlign: 'right' }}>
            <div style={{ ...sx.serifDisplay, fontSize: 24, color: Hearth.ink, lineHeight: 1 }}>€{p.spent}</div>
            <div style={{ ...sx.sans, fontSize: 11, color: Hearth.clayDeep, fontWeight: 700, marginTop: 2 }}>{p.share}% OF KITTY</div>
          </div>
        </div>
      ))}
      <div style={{ borderTop: `1px solid ${Hearth.ink}`, paddingTop: 16, marginTop: 4, ...sx.sans, fontSize: 13, color: Hearth.ink2 }}>
        Settle-up: Jonah owes Maya <strong style={{ ...sx.mono, color: Hearth.ink }}>€186.40</strong>. Last reconciliation was on the 1st.
      </div>
    </div>
  );
}

// Below-fold: bills + activity
function ColumnsLowerC() {
  return (
    <div style={{ display: 'grid', gridTemplateColumns: '1.4fr 1fr', gap: 32, marginTop: 36, paddingTop: 28, borderTop: `2px solid ${Hearth.ink}` }}>
      <div>
        <div style={{ ...sx.sans, fontSize: 11, fontWeight: 700, letterSpacing: '0.18em', color: Hearth.clayDeep, marginBottom: 8 }}>RECENT · UPDATED 9 MIN AGO</div>
        <div style={{ ...sx.serifDisplay, fontSize: 36, color: Hearth.ink, marginBottom: 18, letterSpacing: '-0.01em' }}>The week's ledger.</div>
        <div style={{ borderTop: `1px solid ${Hearth.border}` }}>
          {recentExpenses.slice(0, 6).map((e, i) => (
            <div key={i} style={{ display: 'grid', gridTemplateColumns: '90px 1fr 110px 110px', gap: 18, padding: '14px 0', borderBottom: `1px solid ${Hearth.border}`, alignItems: 'center' }}>
              <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, fontWeight: 700, letterSpacing: '0.1em' }}>{e.when.split('·')[0].trim().toUpperCase()}</div>
              <div>
                <div style={{ ...sx.serifDisplay, fontSize: 18, color: Hearth.ink, lineHeight: 1.2 }}>{e.label}</div>
                <div style={{ ...sx.sans, fontSize: 12, color: Hearth.mute }}>{e.merch} · {e.cat.name}</div>
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                <Avatar name={e.who} color={e.whoColor} size={22} />
                <span style={{ ...sx.sans, fontSize: 12, color: Hearth.ink2 }}>{e.who}</span>
              </div>
              <div style={{ ...sx.mono, fontSize: 17, color: Hearth.ink, textAlign: 'right' }}>€{e.amt.toFixed(2)}</div>
            </div>
          ))}
        </div>
      </div>

      <div>
        <div style={{ ...sx.sans, fontSize: 11, fontWeight: 700, letterSpacing: '0.18em', color: Hearth.clayDeep, marginBottom: 8 }}>UPCOMING · 14-DAY HORIZON</div>
        <div style={{ ...sx.serifDisplay, fontSize: 36, color: Hearth.ink, marginBottom: 18, letterSpacing: '-0.01em' }}>What's coming next.</div>
        <div style={{ borderTop: `1px solid ${Hearth.border}` }}>
          {[
            { day: 'FRI', date: '15 SEP', label: 'Rent · Velázquez', amt: 1450, hot: true },
            { day: 'MON', date: '18 SEP', label: 'Iris · swimming Q4', amt: 165 },
            { day: 'FRI', date: '22 SEP', label: 'Maya · dentist', amt: 80 },
            { day: 'THU', date: '28 SEP', label: 'Internet · Movistar', amt: 39.90 },
          ].map((b, i) => (
            <div key={i} style={{ display: 'grid', gridTemplateColumns: '60px 1fr 90px', gap: 12, padding: '14px 0', borderBottom: `1px solid ${Hearth.border}`, alignItems: 'center' }}>
              <div>
                <div style={{ ...sx.sans, fontSize: 11, color: b.hot ? Hearth.clayDeep : Hearth.mute, fontWeight: 700, letterSpacing: '0.1em' }}>{b.day}</div>
                <div style={{ ...sx.serifDisplay, fontSize: 18, color: Hearth.ink, lineHeight: 1 }}>{b.date.split(' ')[0]}</div>
              </div>
              <div>
                <div style={{ ...sx.serifDisplay, fontSize: 17, color: Hearth.ink, lineHeight: 1.2 }}>{b.label}</div>
                {b.hot && <div style={{ ...sx.sans, fontSize: 10, color: Hearth.clayDeep, fontWeight: 700, letterSpacing: '0.12em', marginTop: 2 }}>IN 3 DAYS</div>}
              </div>
              <div style={{ ...sx.mono, fontSize: 16, color: Hearth.ink, textAlign: 'right' }}>€{b.amt.toFixed(2)}</div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

function DashboardC() {
  return (
    <div style={{ background: Hearth.paper, color: Hearth.ink, ...sx.sans }}>
      <AppNav />
      <MastheadC />
      <div style={{ padding: '32px 48px 48px', display: 'grid', gridTemplateColumns: '1.6fr 1fr 1fr', gap: 36 }}>
        <LeadStoryC />
        <CategoriesColC />
        <MembersColC />
      </div>
      <div style={{ padding: '0 48px 48px' }}>
        <ColumnsLowerC />
      </div>
    </div>
  );
}

window.DashboardC = DashboardC;
