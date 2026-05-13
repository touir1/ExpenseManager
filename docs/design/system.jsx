// Brand system overview artboard — palette, type, components for the Hearth direction.

function SwatchRow({ items, label }) {
  return (
    <div>
      <div style={{ ...sx.sans, fontSize: 11, letterSpacing: '0.12em', textTransform: 'uppercase', color: Hearth.mute, marginBottom: 10 }}>{label}</div>
      <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
        {items.map(s => (
          <div key={s.name} style={{ width: 132 }}>
            <div style={{
              height: 76, borderRadius: 14, background: s.hex,
              border: `1px solid ${Hearth.border}`,
              boxShadow: '0 1px 0 rgba(0,0,0,.03) inset',
            }} />
            <div style={{ ...sx.sans, fontWeight: 600, fontSize: 13, color: Hearth.ink, marginTop: 8 }}>{s.name}</div>
            <div style={{ ...sx.mono, fontSize: 11, color: Hearth.mute }}>{s.hex}</div>
          </div>
        ))}
      </div>
    </div>
  );
}

function TypeSpec({ family, label, sample, size, weight = 400, letter = '-0.01em', color = Hearth.ink, lineH = 1.05, italic = false }) {
  return (
    <div style={{ borderTop: `1px solid ${Hearth.border}`, padding: '20px 0' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', marginBottom: 10 }}>
        <div style={{ ...sx.sans, fontSize: 11, letterSpacing: '0.12em', textTransform: 'uppercase', color: Hearth.mute }}>{label}</div>
        <div style={{ ...sx.mono, fontSize: 11, color: Hearth.mute }}>{family.split(',')[0].replace(/"/g,'')} · {size}px · {weight}{italic ? ' italic' : ''}</div>
      </div>
      <div style={{ fontFamily: family, fontSize: size, fontWeight: weight, letterSpacing: letter, lineHeight: lineH, color, fontStyle: italic ? 'italic' : 'normal' }}>
        {sample}
      </div>
    </div>
  );
}

function ButtonExample() {
  const baseBtn = { ...sx.sans, fontWeight: 600, fontSize: 14, padding: '11px 18px', borderRadius: 12, border: 'none', cursor: 'pointer', display: 'inline-flex', alignItems: 'center', gap: 8 };
  return (
    <div style={{ display: 'flex', gap: 10, alignItems: 'center', flexWrap: 'wrap' }}>
      <button style={{ ...baseBtn, background: Hearth.clay, color: '#fff', boxShadow: '0 1px 0 rgba(255,255,255,.25) inset, 0 4px 12px rgba(200,98,62,.25)' }}>
        Add expense <Icon.plus size={14} />
      </button>
      <button style={{ ...baseBtn, background: Hearth.card, color: Hearth.ink, border: `1px solid ${Hearth.border}` }}>Secondary</button>
      <button style={{ ...baseBtn, background: 'transparent', color: Hearth.clayDeep }}>Quiet link <Icon.arrowRight size={14} /></button>
    </div>
  );
}

function CardExample() {
  return (
    <div style={{
      background: Hearth.card, border: `1px solid ${Hearth.border}`,
      borderRadius: 20, padding: 20, boxShadow: '0 1px 0 rgba(0,0,0,.02), 0 8px 24px -16px rgba(60,40,20,.18)',
      maxWidth: 280,
    }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <div>
          <div style={{ ...sx.sans, fontSize: 12, color: Hearth.mute, fontWeight: 600, letterSpacing: '0.04em', textTransform: 'uppercase' }}>This month</div>
          <div style={{ ...sx.serifDisplay, fontSize: 38, color: Hearth.ink, marginTop: 6 }}>€2,430<span style={{ color: Hearth.mute, fontSize: 22 }}>.50</span></div>
        </div>
        <Pill bg={Hearth.sageSoft} color="#3F5C32"><Icon.arrowDown size={11} /> 8%</Pill>
      </div>
      <div style={{ marginTop: 14, height: 6, borderRadius: 999, background: Hearth.cardAlt, overflow: 'hidden' }}>
        <div style={{ width: '64%', height: '100%', background: Hearth.clay }} />
      </div>
      <div style={{ ...sx.sans, fontSize: 12, color: Hearth.mute, marginTop: 8 }}>64% of €3,800 budget · 18 days left</div>
    </div>
  );
}

function BrandSystemArtboard() {
  const palette = [
    { name: 'Paper',  hex: Hearth.paper },
    { name: 'Card',   hex: Hearth.card },
    { name: 'Border', hex: Hearth.border },
    { name: 'Ink',    hex: Hearth.ink },
    { name: 'Body',   hex: Hearth.ink2 },
  ];
  const accents = [
    { name: 'Clay',    hex: Hearth.clay },
    { name: 'Clay deep', hex: Hearth.clayDeep },
    { name: 'Clay soft', hex: Hearth.claySoft },
    { name: 'Sage',    hex: Hearth.sage },
    { name: 'Berry',   hex: Hearth.berry },
    { name: 'Mustard', hex: Hearth.mustard },
    { name: 'Sky',     hex: Hearth.sky },
  ];
  return (
    <div style={{ background: Hearth.paper, color: Hearth.ink, ...sx.sans, padding: 56, minHeight: 980 }}>
      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'flex-end', justifyContent: 'space-between', marginBottom: 36, gap: 40 }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 18 }}>
            <HearthMark size={40} />
            <Wordmark size={24} />
          </div>
          <div style={{ ...sx.serifDisplay, fontSize: 68, lineHeight: 1.0, color: Hearth.ink }}>
            A warmer way<br/>to keep the books.
          </div>
          <div style={{ marginTop: 18, fontSize: 16, color: Hearth.ink2, maxWidth: 540, lineHeight: 1.55 }}>
            ExpensesManager, redesigned as <em style={{ ...sx.serifDisplay, fontStyle: 'italic', fontSize: 18 }}>Hearth</em> — a family money app that feels like the kitchen counter, not the trading floor. Cream paper, clay accents, hand-set serif moments.
          </div>
        </div>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 10, alignItems: 'flex-end' }}>
          <Pill bg={Hearth.claySofter} color={Hearth.clayDeep}>Direction · Hearth</Pill>
          <Pill bg={Hearth.sageSoft} color="#3F5C32">For families with kids</Pill>
          <Pill bg={Hearth.mustardSoft} color="#7A5C1F">Warm & human tone</Pill>
        </div>
      </div>

      {/* Two columns */}
      <div style={{ display: 'grid', gridTemplateColumns: '1.2fr 1fr', gap: 56 }}>
        {/* Left: palette + type */}
        <div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 32 }}>
            <SwatchRow label="Surface neutrals" items={palette} />
            <SwatchRow label="Accents" items={accents} />
          </div>

          <div style={{ marginTop: 40 }}>
            <div style={{ ...sx.sans, fontSize: 11, letterSpacing: '0.12em', textTransform: 'uppercase', color: Hearth.mute, marginBottom: 6 }}>Typography</div>
            <TypeSpec label="Display"   family={sx.serifDisplay.fontFamily} size={56} sample="What did the week cost us?" />
            <TypeSpec label="Display italic" italic family={sx.serifDisplay.fontFamily} size={32} sample="Hearth is for households." />
            <TypeSpec label="Heading"   family={sx.sans.fontFamily} weight={700} letter="-0.02em" size={22} sample="This month · September" />
            <TypeSpec label="Body"      family={sx.sans.fontFamily} weight={400} letter="0" lineH={1.55} size={15} color={Hearth.ink2} sample="Add an expense and we'll sort it. Split between Maya and Jonah, tagged Groceries — easy." />
            <TypeSpec label="Amounts"   family={sx.mono.fontFamily} weight={500} letter="-0.01em" size={28} sample="€2,430.50  ·  €87.20  ·  −€12.00" />
          </div>
        </div>

        {/* Right: components + voice */}
        <div>
          <div style={{ ...sx.sans, fontSize: 11, letterSpacing: '0.12em', textTransform: 'uppercase', color: Hearth.mute, marginBottom: 14 }}>Buttons</div>
          <ButtonExample />

          <div style={{ ...sx.sans, fontSize: 11, letterSpacing: '0.12em', textTransform: 'uppercase', color: Hearth.mute, marginBottom: 14, marginTop: 36 }}>Cards & data</div>
          <CardExample />

          <div style={{ ...sx.sans, fontSize: 11, letterSpacing: '0.12em', textTransform: 'uppercase', color: Hearth.mute, marginBottom: 14, marginTop: 36 }}>Category language</div>
          <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
            {Categories.map(c => (
              <Pill key={c.id} bg={c.soft} color={c.color} size="md">
                <span style={{ fontSize: 14 }}>{c.glyph}</span> {c.name}
              </Pill>
            ))}
          </div>

          <div style={{ ...sx.sans, fontSize: 11, letterSpacing: '0.12em', textTransform: 'uppercase', color: Hearth.mute, marginBottom: 14, marginTop: 36 }}>Voice — before & after</div>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
            <div style={{ padding: 16, borderRadius: 14, border: `1px dashed ${Hearth.border}`, background: 'transparent' }}>
              <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, fontWeight: 700, marginBottom: 8 }}>BEFORE</div>
              <div style={{ ...sx.sans, fontSize: 14, color: Hearth.ink2, lineHeight: 1.5 }}>"Welcome back. Track your expenses with our comprehensive platform."</div>
            </div>
            <div style={{ padding: 16, borderRadius: 14, background: Hearth.claySofter }}>
              <div style={{ ...sx.sans, fontSize: 11, color: Hearth.clayDeep, fontWeight: 700, marginBottom: 8 }}>AFTER</div>
              <div style={{ ...sx.serifDisplay, fontSize: 19, color: Hearth.ink, lineHeight: 1.35 }}>"Morning, Maya. The grocery run on Sunday — was that on the joint card?"</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

window.BrandSystemArtboard = BrandSystemArtboard;
