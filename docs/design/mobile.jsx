// Mobile screens — phone in iOS frame.

function MobileHome() {
  return (
    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', background: Hearth.cardAlt, padding: '20px 0', minHeight: 900 }}>
      <IOSDevice width={380} height={820} title="Hearth" dark={false}>
        <div style={{ background: Hearth.paper, height: '100%', overflow: 'hidden', ...sx.sans }}>
          {/* Greeting */}
          <div style={{ padding: '12px 18px 0', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <Avatar name="Maya R" color={Hearth.clay} size={36} />
              <div>
                <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, fontWeight: 600 }}>The Rivera family</div>
                <div style={{ ...sx.sans, fontSize: 14, color: Hearth.ink, fontWeight: 700 }}>Hello, Maya</div>
              </div>
            </div>
            <div style={{ display: 'flex', gap: 10, color: Hearth.ink }}>
              <Icon.search size={20} />
              <Icon.bell size={20} />
            </div>
          </div>

          {/* Hero */}
          <div style={{ padding: '14px 18px 0' }}>
            <div style={{
              borderRadius: 24, background: `linear-gradient(160deg, ${Hearth.claySofter} 0%, ${Hearth.card} 90%)`,
              border: `1px solid ${Hearth.border}`, padding: 20,
            }}>
              <div style={{ ...sx.sans, fontSize: 10, color: Hearth.clayDeep, fontWeight: 700, letterSpacing: '0.14em' }}>SPENT IN SEPTEMBER</div>
              <div style={{ ...sx.serifDisplay, fontSize: 52, color: Hearth.ink, lineHeight: 1, marginTop: 4, letterSpacing: '-0.02em' }}>
                €2,430<span style={{ color: Hearth.mute, fontSize: 30 }}>.50</span>
              </div>
              <div style={{ ...sx.sans, fontSize: 12, color: Hearth.ink2, marginTop: 6 }}>€1,370 left of €3,800 · 18 days to go</div>

              <div style={{ marginTop: 12, height: 6, borderRadius: 999, background: Hearth.card, overflow: 'hidden' }}>
                <div style={{ width: '64%', height: '100%', background: Hearth.clay }} />
              </div>

              <div style={{ display: 'flex', gap: 8, marginTop: 14 }}>
                <Pill bg={Hearth.sageSoft} color="#3F5C32"><Icon.arrowDown size={10} /> 8% vs Aug</Pill>
                <Pill bg={Hearth.mustardSoft} color="#7A5C1F"><Icon.spark size={11} /> 5-week streak</Pill>
              </div>
            </div>
          </div>

          {/* Category strip */}
          <div style={{ padding: '14px 18px 0' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
              <div style={{ ...sx.sans, fontSize: 11, fontWeight: 700, letterSpacing: '0.14em', color: Hearth.mute }}>WHERE IT WENT</div>
              <span style={{ ...sx.sans, fontSize: 12, color: Hearth.clayDeep, fontWeight: 600 }}>All →</span>
            </div>
            <div style={{ display: 'flex', gap: 8, overflowX: 'auto', paddingBottom: 4 }}>
              {[
                { c: Categories[0], v: 870 },
                { c: Categories[2], v: 530 },
                { c: Categories[1], v: 390 },
                { c: Categories[3], v: 290 },
                { c: Categories[4], v: 195 },
              ].map(it => (
                <div key={it.c.id} style={{
                  flexShrink: 0, padding: '12px 14px', background: Hearth.card, border: `1px solid ${Hearth.border}`,
                  borderRadius: 16, minWidth: 100,
                }}>
                  <div style={{ fontSize: 20, marginBottom: 6 }}>{it.c.glyph}</div>
                  <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, fontWeight: 600 }}>{it.c.name}</div>
                  <div style={{ ...sx.serifDisplay, fontSize: 22, color: Hearth.ink, marginTop: 2 }}>€{it.v}</div>
                </div>
              ))}
            </div>
          </div>

          {/* Recent feed */}
          <div style={{ padding: '14px 18px 12px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 6 }}>
              <div style={{ ...sx.sans, fontSize: 11, fontWeight: 700, letterSpacing: '0.14em', color: Hearth.mute }}>TODAY</div>
              <span style={{ ...sx.sans, fontSize: 12, color: Hearth.clayDeep, fontWeight: 600 }}>See all →</span>
            </div>
            <div style={{ background: Hearth.card, borderRadius: 18, border: `1px solid ${Hearth.border}`, padding: '4px 14px' }}>
              {[
                ['🛒', 'Weekly shop', 'Maya · split 3', 80.90, Hearth.clay],
                ['🚗', 'Cab to airport', 'Jonah', 24.50, Hearth.sage],
                ['📚', 'Roald Dahl book', 'Iris · pocket', 12.40, Hearth.mustard],
              ].map(([g, l, s, a, c], i) => (
                <div key={i} style={{ display: 'flex', alignItems: 'center', gap: 12, padding: '12px 0', borderBottom: i < 2 ? `1px solid ${Hearth.divider}` : 'none' }}>
                  <div style={{ width: 36, height: 36, borderRadius: 12, background: Hearth.cardAlt, display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: 18 }}>{g}</div>
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{ ...sx.sans, fontSize: 14, color: Hearth.ink, fontWeight: 600 }}>{l}</div>
                    <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute }}>{s}</div>
                  </div>
                  <div style={{ ...sx.mono, fontSize: 14, color: Hearth.ink, fontWeight: 500 }}>€{a.toFixed(2)}</div>
                </div>
              ))}
            </div>
          </div>

          {/* Bottom tab bar */}
          <div style={{
            position: 'absolute', bottom: 28, left: 18, right: 18,
            background: 'rgba(255,252,246,0.95)', backdropFilter: 'blur(12px)',
            border: `1px solid ${Hearth.border}`, borderRadius: 24,
            padding: '12px 8px', display: 'flex', justifyContent: 'space-around', alignItems: 'center',
            boxShadow: '0 12px 32px -12px rgba(30,20,10,.18)',
          }}>
            {[
              { i: '🏠', l: 'Home', a: true },
              { i: '🧾', l: 'Expenses' },
              { i: '👨‍👩‍👧', l: 'Family' },
              { i: '⚙️', l: 'Settings' },
            ].map(t => (
              <div key={t.l} style={{ textAlign: 'center', flex: 1, color: t.a ? Hearth.clayDeep : Hearth.mute }}>
                <div style={{ fontSize: 18 }}>{t.i}</div>
                <div style={{ ...sx.sans, fontSize: 10, fontWeight: 700, marginTop: 2 }}>{t.l}</div>
              </div>
            ))}
            <div style={{ position: 'absolute', right: -4, top: -22, width: 56, height: 56, borderRadius: '50%', background: Hearth.clay, color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center', boxShadow: `0 12px 24px -6px ${Hearth.clay}`, border: '3px solid #FFFCF6' }}><Icon.plus size={22} /></div>
          </div>
        </div>
      </IOSDevice>
    </div>
  );
}

function MobileAdd() {
  return (
    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', background: Hearth.cardAlt, padding: '20px 0', minHeight: 900 }}>
      <IOSDevice width={380} height={820} title="New expense" dark={false}>
        <div style={{ background: Hearth.paper, height: '100%', ...sx.sans, position: 'relative' }}>
          <div style={{ padding: '8px 18px 0', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span style={{ ...sx.sans, fontSize: 14, color: Hearth.mute }}>Cancel</span>
            <div style={{ ...sx.serifDisplay, fontSize: 22, color: Hearth.ink }}>New expense</div>
            <span style={{ ...sx.sans, fontSize: 14, color: Hearth.clayDeep, fontWeight: 700 }}>Save</span>
          </div>

          {/* Big amount */}
          <div style={{ padding: '32px 18px 20px', textAlign: 'center', borderBottom: `1px dashed ${Hearth.border}` }}>
            <div style={{ ...sx.serifDisplay, fontSize: 76, color: Hearth.ink, lineHeight: 1, letterSpacing: '-0.02em' }}>
              <span style={{ color: Hearth.mute, fontSize: 50 }}>€</span>80<span style={{ color: Hearth.mute }}>.90</span>
            </div>
            <div style={{ ...sx.sans, fontSize: 12, color: Hearth.mute, marginTop: 8 }}>EUR · tap to change currency</div>
          </div>

          {/* What */}
          <div style={{ padding: '16px 18px 0' }}>
            <input style={{
              ...sx.sans, width: '100%', boxSizing: 'border-box', padding: '12px 14px', borderRadius: 12,
              background: Hearth.card, border: `1px solid ${Hearth.border}`, fontSize: 15, color: Hearth.ink, outline: 'none',
            }} defaultValue="The weekly shop" />
          </div>

          {/* Category chips */}
          <div style={{ padding: '14px 18px 0' }}>
            <div style={{ ...sx.sans, fontSize: 10, fontWeight: 700, letterSpacing: '0.14em', color: Hearth.mute, marginBottom: 8 }}>CATEGORY</div>
            <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap' }}>
              {Categories.slice(0,6).map((c,i) => (
                <span key={c.id} style={{
                  display: 'inline-flex', alignItems: 'center', gap: 4,
                  padding: '7px 10px', borderRadius: 999,
                  background: i === 0 ? c.color : c.soft, color: i === 0 ? '#fff' : c.color,
                  ...sx.sans, fontSize: 12, fontWeight: 600,
                }}>{c.glyph} {c.name}</span>
              ))}
            </div>
          </div>

          {/* Who paid */}
          <div style={{ padding: '14px 18px 0' }}>
            <div style={{ ...sx.sans, fontSize: 10, fontWeight: 700, letterSpacing: '0.14em', color: Hearth.mute, marginBottom: 8 }}>WHO PAID</div>
            <div style={{ display: 'flex', gap: 8 }}>
              {[
                { n: 'Maya', c: Hearth.clay, on: true },
                { n: 'Jonah', c: Hearth.sage },
                { n: 'Iris', c: Hearth.mustard },
              ].map(m => (
                <div key={m.n} style={{
                  flex: 1, padding: '10px 6px', textAlign: 'center', borderRadius: 14,
                  background: m.on ? m.c : Hearth.card, color: m.on ? '#fff' : Hearth.ink,
                  border: `1px solid ${m.on ? m.c : Hearth.border}`,
                }}>
                  <Avatar name={m.n} color={m.on ? '#fff' : m.c} size={22} />
                  <div style={{ ...sx.sans, fontSize: 12, fontWeight: 700, marginTop: 4, color: m.on ? '#fff' : Hearth.ink }}>{m.n}</div>
                </div>
              ))}
            </div>
          </div>

          {/* Split */}
          <div style={{ padding: '14px 18px 0' }}>
            <div style={{ ...sx.sans, fontSize: 10, fontWeight: 700, letterSpacing: '0.14em', color: Hearth.mute, marginBottom: 8 }}>SPLIT</div>
            <div style={{ background: Hearth.claySofter, borderRadius: 14, padding: 12 }}>
              <div style={{ ...sx.sans, fontSize: 12, color: Hearth.clayDeep, fontWeight: 600, marginBottom: 8 }}>Evenly · 3 ways · €26.97 each</div>
              <div style={{ display: 'flex', gap: 4 }}>
                {[Hearth.clay, Hearth.sage, Hearth.mustard].map((c,i) => (
                  <div key={i} style={{ flex: 1, height: 6, background: c, borderRadius: 6 }} />
                ))}
              </div>
            </div>
          </div>

          {/* Hint */}
          <div style={{ padding: '20px 18px', ...sx.sans, fontSize: 12, color: Hearth.mute, textAlign: 'center' }}>
            <Icon.spark size={12} /> Looks like Saturday's grocery run. We pre-filled the category.
          </div>
        </div>
      </IOSDevice>
    </div>
  );
}

window.MobileHome = MobileHome;
window.MobileAdd = MobileAdd;
