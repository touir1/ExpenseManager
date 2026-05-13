// Marketing / landing page artboard — the public-facing front door.

function MarketingNav() {
  return (
    <div style={{
      display: 'flex', alignItems: 'center', justifyContent: 'space-between',
      padding: '24px 56px', borderBottom: `1px solid ${Hearth.border}`,
      background: 'rgba(250,246,238,0.85)', backdropFilter: 'blur(8px)',
      position: 'sticky', top: 0, zIndex: 5,
    }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
        <HearthMark size={32} />
        <Wordmark size={19} />
      </div>
      <div style={{ display: 'flex', gap: 28, alignItems: 'center', ...sx.sans, fontSize: 14, color: Hearth.ink2 }}>
        <a style={{ color: 'inherit', textDecoration: 'none' }}>How it works</a>
        <a style={{ color: 'inherit', textDecoration: 'none' }}>For families</a>
        <a style={{ color: 'inherit', textDecoration: 'none' }}>Pricing</a>
        <a style={{ color: 'inherit', textDecoration: 'none' }}>Help</a>
      </div>
      <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
        <button style={{ ...sx.sans, background: 'transparent', border: 'none', color: Hearth.ink, fontSize: 14, fontWeight: 600, cursor: 'pointer' }}>Sign in</button>
        <button style={{
          ...sx.sans, fontWeight: 600, fontSize: 14, padding: '10px 18px',
          background: Hearth.ink, color: Hearth.paper, border: 'none', borderRadius: 999, cursor: 'pointer',
          display: 'inline-flex', alignItems: 'center', gap: 8,
        }}>Start free <Icon.arrowRight size={14} /></button>
      </div>
    </div>
  );
}

// A stylized hero "receipt" — the marketing visual.
function HeroReceipt() {
  return (
    <div style={{
      position: 'relative', width: 460, transform: 'rotate(-3deg)',
      background: Hearth.card, borderRadius: 22, padding: 28,
      boxShadow: '0 30px 60px -30px rgba(60,30,10,.35), 0 8px 24px -12px rgba(60,30,10,.15)',
      border: `1px solid ${Hearth.border}`,
    }}>
      {/* tape */}
      <div style={{ position: 'absolute', top: -16, left: 60, width: 80, height: 28, background: 'rgba(214,162,63,.45)', transform: 'rotate(-6deg)', borderRadius: 2, boxShadow: '0 2px 6px rgba(0,0,0,.06)' }} />

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 18 }}>
        <div>
          <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, fontWeight: 700, letterSpacing: '0.12em' }}>SATURDAY · 12 SEP</div>
          <div style={{ ...sx.serifDisplay, fontSize: 28, color: Hearth.ink, marginTop: 4 }}>The weekly shop</div>
        </div>
        <Pill bg={Categories[0].soft} color={Categories[0].color}>🛒 Groceries</Pill>
      </div>

      <div style={{ borderTop: `1px dashed ${Hearth.border}`, borderBottom: `1px dashed ${Hearth.border}`, padding: '14px 0', display: 'flex', flexDirection: 'column', gap: 8 }}>
        {[
          ['Sourdough, eggs, milk', 11.40],
          ['School-lunch supplies', 23.80],
          ['Sunday roast', 38.50],
          ['Apples, pears, carrots', 7.20],
        ].map(([label, amount]) => (
          <div key={label} style={{ display: 'flex', justifyContent: 'space-between', ...sx.sans, fontSize: 14, color: Hearth.ink2 }}>
            <span>{label}</span>
            <span style={{ ...sx.mono }}>€{amount.toFixed(2)}</span>
          </div>
        ))}
      </div>

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', marginTop: 16 }}>
        <div style={{ ...sx.sans, fontSize: 13, color: Hearth.mute }}>Paid by <strong style={{ color: Hearth.ink }}>Maya</strong></div>
        <div style={{ ...sx.serifDisplay, fontSize: 32, color: Hearth.ink }}>€80.90</div>
      </div>

      <div style={{ marginTop: 14, padding: 12, background: Hearth.claySofter, borderRadius: 12, display: 'flex', alignItems: 'center', gap: 10 }}>
        <Avatar name="Maya R" color={Hearth.clay} size={24} />
        <Avatar name="Jonah R" color={Hearth.sage} size={24} />
        <Avatar name="Iris" color={Hearth.mustard} size={24} />
        <div style={{ ...sx.sans, fontSize: 12, color: Hearth.clayDeep, fontWeight: 600 }}>Split 3 ways · €26.97 each</div>
      </div>
    </div>
  );
}

// Mini illustrative phone — purely visual
function HeroPhone() {
  return (
    <div style={{
      width: 240, height: 480, borderRadius: 38,
      background: Hearth.ink, padding: 8, transform: 'rotate(5deg)',
      boxShadow: '0 30px 60px -20px rgba(60,30,10,.4)',
    }}>
      <div style={{ width: '100%', height: '100%', borderRadius: 32, background: Hearth.paper, overflow: 'hidden', position: 'relative', padding: 16 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', ...sx.sans, fontSize: 11, color: Hearth.ink, marginBottom: 16 }}>
          <span style={{ fontWeight: 700 }}>9:41</span>
          <span style={{ display: 'flex', gap: 4 }}><Icon.dot size={6} /><Icon.dot size={6} /><Icon.dot size={6} /></span>
        </div>
        <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, fontWeight: 700 }}>THIS MONTH</div>
        <div style={{ ...sx.serifDisplay, fontSize: 44, color: Hearth.ink, marginTop: 4, lineHeight: 1 }}>€2,430</div>
        <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, marginTop: 4 }}>€1,370 left of €3,800</div>

        <div style={{ marginTop: 16, padding: 14, background: Hearth.card, borderRadius: 16, border: `1px solid ${Hearth.border}` }}>
          {[
            ['🛒', 'The weekly shop', '€80.90'],
            ['🎒', 'Iris swim class', '€42.00'],
            ['🍝', 'Tuesday takeaway', '€28.50'],
          ].map(([g, l, a], i) => (
            <div key={i} style={{ display: 'flex', alignItems: 'center', gap: 10, padding: '8px 0', borderBottom: i < 2 ? `1px solid ${Hearth.divider}` : 'none' }}>
              <span style={{ fontSize: 16 }}>{g}</span>
              <div style={{ flex: 1, ...sx.sans, fontSize: 12, color: Hearth.ink }}>{l}</div>
              <span style={{ ...sx.mono, fontSize: 12, color: Hearth.ink }}>{a}</span>
            </div>
          ))}
        </div>

        {/* Floating FAB */}
        <div style={{
          position: 'absolute', bottom: 24, right: 18,
          width: 52, height: 52, borderRadius: '50%', background: Hearth.clay,
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          boxShadow: '0 8px 20px rgba(200,98,62,.4)', color: '#fff',
        }}><Icon.plus size={22} /></div>
      </div>
    </div>
  );
}

function MarketingHero() {
  return (
    <div style={{ padding: '80px 56px 60px', display: 'grid', gridTemplateColumns: '1.05fr 1fr', gap: 60, alignItems: 'center' }}>
      <div>
        <Pill bg={Hearth.mustardSoft} color="#7A5C1F" size="md">
          <span style={{ width: 6, height: 6, borderRadius: 999, background: Hearth.mustard, display: 'inline-block' }} /> Now with budgets that follow your week
        </Pill>
        <h1 style={{
          ...sx.serifDisplay, fontSize: 92, lineHeight: 0.98, margin: '24px 0 22px',
          color: Hearth.ink, letterSpacing: '-0.02em',
        }}>
          The family<br/>
          <em style={{ ...sx.serifDisplay, fontStyle: 'italic', color: Hearth.clay }}>money jar</em>,<br/>
          but smarter.
        </h1>
        <p style={{ ...sx.sans, fontSize: 19, lineHeight: 1.55, color: Hearth.ink2, maxWidth: 520, marginBottom: 32 }}>
          Track every euro the household spends — together. Split the grocery run, see what swim lessons cost this term, and end the month without the spreadsheet argument.
        </p>
        <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
          <button style={{
            ...sx.sans, fontWeight: 600, fontSize: 16, padding: '14px 22px',
            background: Hearth.ink, color: Hearth.paper, border: 'none', borderRadius: 999,
            display: 'inline-flex', alignItems: 'center', gap: 10, cursor: 'pointer',
            boxShadow: '0 8px 20px -4px rgba(30,20,10,.25)',
          }}>Start free — for the whole family <Icon.arrowRight size={16} /></button>
          <button style={{
            ...sx.sans, fontWeight: 600, fontSize: 15, padding: '14px 18px',
            background: 'transparent', color: Hearth.ink, border: 'none', cursor: 'pointer',
          }}>See a 60-second tour →</button>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 14, marginTop: 32, ...sx.sans, fontSize: 13, color: Hearth.mute }}>
          <div style={{ display: 'flex' }}>
            {[Hearth.clay, Hearth.sage, Hearth.mustard, Hearth.sky].map((c,i) => (
              <span key={i} style={{ marginLeft: i ? -8 : 0 }}><Avatar name={['M R','J R','I','A'][i]} color={c} size={28} ring /></span>
            ))}
          </div>
          <div>Free forever for up to 4 family members · No card required</div>
        </div>
      </div>

      <div style={{ position: 'relative', height: 560, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        {/* Soft background blob */}
        <div style={{
          position: 'absolute', inset: 0,
          background: `radial-gradient(60% 60% at 50% 50%, ${Hearth.claySofter} 0%, transparent 70%)`,
        }} />
        <div style={{ position: 'absolute', left: 0, top: 40 }}><HeroReceipt /></div>
        <div style={{ position: 'absolute', right: 30, bottom: 0 }}><HeroPhone /></div>
        {/* Small floating note */}
        <div style={{
          position: 'absolute', right: 0, top: 30, width: 200, padding: 14,
          background: Hearth.mustardSoft, borderRadius: 14, transform: 'rotate(4deg)',
          ...sx.serifDisplay, fontSize: 17, color: '#5C4117', boxShadow: '0 12px 24px -12px rgba(60,30,10,.2)',
        }}>
          "Did Iris's piano teacher get paid?"<br/>
          <span style={{ ...sx.sans, fontStyle: 'normal', fontSize: 11, color: '#7A5C1F', fontWeight: 700, letterSpacing: '0.08em' }}>— EVERY THURSDAY</span>
        </div>
      </div>
    </div>
  );
}

function LogoStrip() {
  return (
    <div style={{ padding: '32px 56px', borderTop: `1px solid ${Hearth.border}`, borderBottom: `1px solid ${Hearth.border}`, background: Hearth.cardAlt }}>
      <div style={{ ...sx.sans, fontSize: 12, color: Hearth.mute, letterSpacing: '0.14em', textTransform: 'uppercase', textAlign: 'center', marginBottom: 18 }}>
        Loved by 38,000 households across Europe
      </div>
      <div style={{ display: 'flex', justifyContent: 'space-around', alignItems: 'center', opacity: 0.55, ...sx.serifDisplay, fontSize: 22, color: Hearth.ink2 }}>
        <span>The Household Weekly</span>
        <span>·</span>
        <span style={{ fontStyle: 'italic' }}>Modern Parent</span>
        <span>·</span>
        <span style={{ ...sx.sans, fontWeight: 700, letterSpacing: '0.04em' }}>FAMILY FINANCE</span>
        <span>·</span>
        <span>Kitchen Table Mag.</span>
      </div>
    </div>
  );
}

function FeatureRow({ kicker, title, copy, visual, reverse, color }) {
  return (
    <div style={{
      display: 'grid', gridTemplateColumns: reverse ? '1fr 1.1fr' : '1.1fr 1fr',
      gap: 60, padding: '100px 56px', alignItems: 'center',
      direction: reverse ? 'rtl' : 'ltr',
    }}>
      <div style={{ direction: 'ltr' }}>
        <div style={{ ...sx.sans, fontSize: 12, color, fontWeight: 700, letterSpacing: '0.14em', textTransform: 'uppercase', marginBottom: 12 }}>{kicker}</div>
        <div style={{ ...sx.serifDisplay, fontSize: 54, lineHeight: 1.02, color: Hearth.ink, marginBottom: 18, letterSpacing: '-0.02em' }}>{title}</div>
        <div style={{ ...sx.sans, fontSize: 17, lineHeight: 1.6, color: Hearth.ink2, maxWidth: 460 }}>{copy}</div>
      </div>
      <div style={{ direction: 'ltr' }}>{visual}</div>
    </div>
  );
}

function FeatureCategoryDonut() {
  // simple SVG donut
  const segments = [
    { p: 36, c: Categories[0].color },
    { p: 22, c: Categories[2].color },
    { p: 16, c: Categories[1].color },
    { p: 12, c: Categories[3].color },
    { p: 8,  c: Categories[4].color },
    { p: 6,  c: Categories[5].color },
  ];
  const R = 110, C = 2 * Math.PI * R;
  let offset = 0;
  return (
    <div style={{
      background: Hearth.card, borderRadius: 28, padding: 32, border: `1px solid ${Hearth.border}`,
      boxShadow: '0 20px 60px -30px rgba(60,30,10,.3)',
    }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 36 }}>
        <div style={{ position: 'relative', width: 260, height: 260, flexShrink: 0 }}>
          <svg width={260} height={260} viewBox="0 0 260 260" style={{ transform: 'rotate(-90deg)' }}>
            <circle cx={130} cy={130} r={R} fill="none" stroke={Hearth.cardAlt} strokeWidth={28} />
            {segments.map((s,i) => {
              const len = (s.p / 100) * C;
              const el = <circle key={i} cx={130} cy={130} r={R} fill="none" stroke={s.c} strokeWidth={28}
                strokeDasharray={`${len} ${C - len}`} strokeDashoffset={-offset} strokeLinecap="butt" />;
              offset += len + 4; // small gap
              return el;
            })}
          </svg>
          <div style={{ position: 'absolute', inset: 0, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center' }}>
            <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, fontWeight: 700, letterSpacing: '0.1em' }}>SPENT IN SEP</div>
            <div style={{ ...sx.serifDisplay, fontSize: 44, color: Hearth.ink, lineHeight: 1 }}>€2,430</div>
            <div style={{ ...sx.sans, fontSize: 12, color: Hearth.sage, fontWeight: 600, marginTop: 4 }}>−8% vs Aug</div>
          </div>
        </div>
        <div style={{ flex: 1, display: 'flex', flexDirection: 'column', gap: 12 }}>
          {Categories.slice(0,6).map((c,i) => (
            <div key={c.id} style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <span style={{ width: 10, height: 10, borderRadius: 3, background: c.color }} />
              <span style={{ ...sx.sans, fontSize: 14, color: Hearth.ink2, flex: 1 }}>{c.name}</span>
              <span style={{ ...sx.mono, fontSize: 13, color: Hearth.ink, fontWeight: 500 }}>{segments[i].p}%</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

function FeatureSplit() {
  return (
    <div style={{
      background: Hearth.card, borderRadius: 28, padding: 32, border: `1px solid ${Hearth.border}`,
      boxShadow: '0 20px 60px -30px rgba(60,30,10,.3)',
    }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 18 }}>
        <div>
          <div style={{ ...sx.sans, fontSize: 12, color: Hearth.mute, fontWeight: 700, letterSpacing: '0.1em' }}>BACK-TO-SCHOOL HAUL</div>
          <div style={{ ...sx.serifDisplay, fontSize: 26, color: Hearth.ink, marginTop: 4 }}>Pencils, books, two new backpacks</div>
        </div>
        <div style={{ ...sx.serifDisplay, fontSize: 36, color: Hearth.ink }}>€184.20</div>
      </div>
      <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginBottom: 18 }}>
        <Pill bg={Categories[1].soft} color={Categories[1].color}>🎒 Kids & school</Pill>
        <Pill bg={Hearth.cardAlt} color={Hearth.ink2}>Paid by Maya</Pill>
      </div>

      <div style={{ ...sx.sans, fontSize: 12, color: Hearth.mute, fontWeight: 700, letterSpacing: '0.1em', marginBottom: 12 }}>SPLIT</div>
      {[
        { name: 'Maya', color: Hearth.clay, p: 50, owes: 'paid' },
        { name: 'Jonah', color: Hearth.sage, p: 50, owes: 92.10 },
      ].map(m => (
        <div key={m.name} style={{ display: 'flex', alignItems: 'center', gap: 12, padding: '10px 0', borderTop: `1px solid ${Hearth.divider}` }}>
          <Avatar name={m.name} color={m.color} size={36} />
          <div style={{ flex: 1, ...sx.sans }}>
            <div style={{ fontSize: 15, fontWeight: 600, color: Hearth.ink }}>{m.name}</div>
            <div style={{ fontSize: 12, color: Hearth.mute }}>{m.p}% share</div>
          </div>
          {m.owes === 'paid'
            ? <Pill bg={Hearth.sageSoft} color="#3F5C32"><Icon.check size={11} /> Paid</Pill>
            : <span style={{ ...sx.mono, fontSize: 16, color: Hearth.clayDeep, fontWeight: 600 }}>owes €{m.owes.toFixed(2)}</span>
          }
        </div>
      ))}

      <div style={{ marginTop: 18, padding: 14, background: Hearth.claySofter, borderRadius: 14, display: 'flex', alignItems: 'center', gap: 12 }}>
        <span style={{ width: 28, height: 28, borderRadius: '50%', background: Hearth.clay, color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center' }}><Icon.spark size={14} /></span>
        <div style={{ ...sx.sans, fontSize: 13, color: Hearth.clayDeep, fontWeight: 500 }}>Iris starts piano next month — we'll watch the Kids budget for you.</div>
      </div>
    </div>
  );
}

function FeatureKidsCorner() {
  return (
    <div style={{
      background: `linear-gradient(160deg, ${Hearth.sageSoft} 0%, ${Hearth.card} 80%)`,
      borderRadius: 28, padding: 32, border: `1px solid ${Hearth.border}`,
      boxShadow: '0 20px 60px -30px rgba(60,30,10,.3)',
    }}>
      <div style={{ ...sx.sans, fontSize: 12, color: '#3F5C32', fontWeight: 700, letterSpacing: '0.12em', marginBottom: 16 }}>KIDS' POCKET MONEY</div>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 14 }}>
        {[
          { name: 'Iris', age: 9, color: '#D6A23F', save: 18.50, spend: 6.20, goal: 'A new scooter', pct: 62 },
          { name: 'Felix', age: 12, color: '#5C8C9E', save: 42.00, spend: 11.40, goal: 'Switch game', pct: 38 },
        ].map(k => (
          <div key={k.name} style={{ background: Hearth.card, borderRadius: 18, padding: 18, border: `1px solid ${Hearth.border}` }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 12 }}>
              <Avatar name={k.name} color={k.color} size={36} />
              <div>
                <div style={{ ...sx.sans, fontSize: 15, color: Hearth.ink, fontWeight: 700 }}>{k.name}</div>
                <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute }}>age {k.age}</div>
              </div>
            </div>
            <div style={{ display: 'flex', gap: 14, marginBottom: 12 }}>
              <div>
                <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, fontWeight: 700, letterSpacing: '0.08em' }}>SAVING</div>
                <div style={{ ...sx.serifDisplay, fontSize: 22, color: Hearth.ink }}>€{k.save.toFixed(2)}</div>
              </div>
              <div>
                <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, fontWeight: 700, letterSpacing: '0.08em' }}>SPENT</div>
                <div style={{ ...sx.serifDisplay, fontSize: 22, color: Hearth.ink2 }}>€{k.spend.toFixed(2)}</div>
              </div>
            </div>
            <div style={{ ...sx.sans, fontSize: 12, color: Hearth.ink2, marginBottom: 6 }}>Toward <strong>{k.goal}</strong></div>
            <div style={{ height: 8, borderRadius: 999, background: Hearth.cardAlt, overflow: 'hidden' }}>
              <div style={{ width: `${k.pct}%`, height: '100%', background: k.color, borderRadius: 999 }} />
            </div>
            <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, marginTop: 6 }}>{k.pct}% of goal</div>
          </div>
        ))}
      </div>
    </div>
  );
}

function FeaturesGrid() {
  const cells = [
    { glyph: '🌍', title: 'Every currency, every trip', body: 'Spending in pounds in London, euros at home, and yen in Tokyo? We do the math at the day\'s rate.' },
    { glyph: '🔁', title: 'Recurring bills, watched', body: 'Rent, music school, gym memberships — we surface them the week before they hit.' },
    { glyph: '🧾', title: 'Snap the receipt', body: 'Photograph it in the queue. We\'ll read the merchant, amount and category.' },
    { glyph: '🔒', title: 'Yours alone, or shared', body: 'You decide what your partner sees. The kids see only their own pockets.' },
    { glyph: '🇪🇸', title: 'Speaks your language', body: 'English, Español, Français, Deutsch — and the labels stay in your tongue.' },
    { glyph: '☁️', title: 'Works on every device', body: 'Phone in the supermarket, laptop on Sunday for the weekly sit-down.' },
  ];
  return (
    <div style={{ padding: '100px 56px', background: Hearth.cardAlt }}>
      <div style={{ textAlign: 'center', marginBottom: 60 }}>
        <div style={{ ...sx.sans, fontSize: 12, color: Hearth.clayDeep, fontWeight: 700, letterSpacing: '0.14em', textTransform: 'uppercase', marginBottom: 12 }}>The whole kit</div>
        <div style={{ ...sx.serifDisplay, fontSize: 60, color: Hearth.ink, lineHeight: 1.02 }}>Everything a household<br/>actually needs.</div>
      </div>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 20, maxWidth: 1100, margin: '0 auto' }}>
        {cells.map(c => (
          <div key={c.title} style={{
            background: Hearth.card, borderRadius: 22, padding: 28, border: `1px solid ${Hearth.border}`,
            boxShadow: '0 1px 0 rgba(0,0,0,.02)',
          }}>
            <div style={{ fontSize: 32, marginBottom: 16 }}>{c.glyph}</div>
            <div style={{ ...sx.serifDisplay, fontSize: 24, color: Hearth.ink, marginBottom: 8 }}>{c.title}</div>
            <div style={{ ...sx.sans, fontSize: 14, lineHeight: 1.55, color: Hearth.ink2 }}>{c.body}</div>
          </div>
        ))}
      </div>
    </div>
  );
}

function Pricing() {
  const tiers = [
    {
      name: 'Hearth Home', price: 'Free', sub: 'forever, for up to 4 in the family',
      perks: ['Shared budget & expenses', 'Up to 4 members', 'Kids\' pocket money', 'Multi-currency', '2 years of history'],
      cta: 'Start free', highlight: false,
    },
    {
      name: 'Hearth Plus', price: '€4.50', sub: 'per month · whole household',
      perks: ['Everything in Home', 'Unlimited members & history', 'Receipt scanning', 'Recurring bill alerts', 'Export to CSV / accountant'],
      cta: 'Try free for 30 days', highlight: true,
    },
    {
      name: 'Hearth Hearth', price: '€9', sub: 'per month · for extended families',
      perks: ['Everything in Plus', 'Multiple households', 'Custom categories', 'Priority human support', 'Beta features early'],
      cta: 'Talk to us', highlight: false,
    },
  ];
  return (
    <div style={{ padding: '100px 56px' }}>
      <div style={{ textAlign: 'center', marginBottom: 50 }}>
        <div style={{ ...sx.sans, fontSize: 12, color: Hearth.clayDeep, fontWeight: 700, letterSpacing: '0.14em', textTransform: 'uppercase', marginBottom: 12 }}>Pricing, plain</div>
        <div style={{ ...sx.serifDisplay, fontSize: 60, color: Hearth.ink, lineHeight: 1 }}>One free plan, two<br/>that buy you time.</div>
      </div>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 16, maxWidth: 1100, margin: '0 auto' }}>
        {tiers.map(t => (
          <div key={t.name} style={{
            background: t.highlight ? Hearth.ink : Hearth.card,
            color: t.highlight ? Hearth.paper : Hearth.ink,
            borderRadius: 24, padding: 32, border: `1px solid ${t.highlight ? Hearth.ink : Hearth.border}`,
            boxShadow: t.highlight ? '0 30px 60px -30px rgba(35,23,14,.6)' : '0 1px 0 rgba(0,0,0,.02)',
            transform: t.highlight ? 'scale(1.04)' : 'none',
          }}>
            {t.highlight && (
              <Pill bg={Hearth.mustard} color={Hearth.ink} size="md" style={{ marginBottom: 16 }}>★ Most families pick this</Pill>
            )}
            <div style={{ ...sx.serifDisplay, fontSize: 28, marginBottom: 6 }}>{t.name}</div>
            <div style={{ display: 'flex', alignItems: 'baseline', gap: 6, marginBottom: 6 }}>
              <span style={{ ...sx.serifDisplay, fontSize: 54, lineHeight: 1 }}>{t.price}</span>
            </div>
            <div style={{ ...sx.sans, fontSize: 13, color: t.highlight ? '#B8AB99' : Hearth.mute, marginBottom: 22 }}>{t.sub}</div>
            <button style={{
              ...sx.sans, fontWeight: 600, fontSize: 14, padding: '12px 16px', width: '100%',
              background: t.highlight ? Hearth.clay : 'transparent',
              color: t.highlight ? '#fff' : Hearth.ink,
              border: t.highlight ? 'none' : `1px solid ${Hearth.border}`,
              borderRadius: 12, cursor: 'pointer', marginBottom: 22,
            }}>{t.cta}</button>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
              {t.perks.map(p => (
                <div key={p} style={{ display: 'flex', gap: 10, alignItems: 'flex-start', ...sx.sans, fontSize: 14, lineHeight: 1.4, color: t.highlight ? '#E0D6C4' : Hearth.ink2 }}>
                  <span style={{ width: 18, height: 18, borderRadius: '50%', background: t.highlight ? 'rgba(214,162,63,.25)' : Hearth.claySofter, color: t.highlight ? Hearth.mustard : Hearth.clayDeep, display: 'inline-flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0, marginTop: 1 }}><Icon.check size={10} /></span>
                  {p}
                </div>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function Testimonials() {
  const quotes = [
    { q: "We stopped fighting about money in 2 weeks. Genuinely.", who: "Aitana & Tomás", where: "Madrid · two kids" },
    { q: "Iris finally understands why we say no to the third toy.", who: "Hannah", where: "Berlin · one kid, two cats" },
    { q: "It looks like a recipe book, not a bank statement. I love that.", who: "Mira", where: "Amsterdam" },
  ];
  return (
    <div style={{ padding: '80px 56px', background: Hearth.claySofter }}>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 24, maxWidth: 1100, margin: '0 auto' }}>
        {quotes.map((t,i) => (
          <div key={i} style={{ padding: 28 }}>
            <div style={{ ...sx.serifDisplay, fontSize: 26, lineHeight: 1.25, color: Hearth.ink, marginBottom: 22, textWrap: 'pretty' }}>
              <span style={{ color: Hearth.clay, marginRight: 4 }}>"</span>{t.q}<span style={{ color: Hearth.clay, marginLeft: 4 }}>"</span>
            </div>
            <div style={{ ...sx.sans, fontSize: 14, fontWeight: 700, color: Hearth.ink }}>{t.who}</div>
            <div style={{ ...sx.sans, fontSize: 12, color: Hearth.ink2 }}>{t.where}</div>
          </div>
        ))}
      </div>
    </div>
  );
}

function FinalCta() {
  return (
    <div style={{ padding: '100px 56px', textAlign: 'center', background: Hearth.ink, color: Hearth.paper }}>
      <div style={{ ...sx.serifDisplay, fontSize: 72, lineHeight: 1, marginBottom: 16, letterSpacing: '-0.02em' }}>
        Let's get the<br/>
        <em style={{ ...sx.serifDisplay, fontStyle: 'italic', color: Hearth.mustard }}>household</em> on the same page.
      </div>
      <div style={{ ...sx.sans, fontSize: 17, color: '#B8AB99', maxWidth: 540, margin: '0 auto 30px' }}>
        Free for up to four. No card. No spreadsheet exports needed — your future self will thank you.
      </div>
      <button style={{
        ...sx.sans, fontWeight: 600, fontSize: 17, padding: '16px 28px',
        background: Hearth.clay, color: '#fff', border: 'none', borderRadius: 999, cursor: 'pointer',
        display: 'inline-flex', alignItems: 'center', gap: 10,
        boxShadow: '0 14px 30px -10px rgba(200,98,62,.6)',
      }}>Start free <Icon.arrowRight size={16} /></button>
    </div>
  );
}

function Footer() {
  return (
    <div style={{ padding: '40px 56px', background: Hearth.ink, color: '#B8AB99', borderTop: '1px solid rgba(255,255,255,0.08)', ...sx.sans, fontSize: 13 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <HearthMark size={26} bg={Hearth.paper} fg={Hearth.ink} />
          <span style={{ color: Hearth.paper, fontWeight: 700 }}>Hearth — formerly ExpensesManager</span>
        </div>
        <div style={{ display: 'flex', gap: 24 }}>
          <a style={{ color: 'inherit', textDecoration: 'none' }}>Privacy</a>
          <a style={{ color: 'inherit', textDecoration: 'none' }}>Terms</a>
          <a style={{ color: 'inherit', textDecoration: 'none' }}>Status</a>
          <a style={{ color: 'inherit', textDecoration: 'none' }}>Hello@hearth.app</a>
        </div>
      </div>
    </div>
  );
}

function MarketingArtboard() {
  return (
    <div style={{ background: Hearth.paper, color: Hearth.ink, ...sx.sans }}>
      <MarketingNav />
      <MarketingHero />
      <LogoStrip />

      <FeatureRow
        kicker="See it at a glance"
        title={<>Where the money <em style={{ ...sx.serifDisplay, fontStyle: 'italic', color: Hearth.clay }}>actually</em> went.</>}
        copy="A simple ring, one color per category. Hover any wedge to see the merchants behind it. No tooltips that look like SQL — just human-shaped answers."
        color={Hearth.clayDeep}
        visual={<FeatureCategoryDonut />}
      />

      <FeatureRow
        kicker="Splits without spreadsheet"
        title={<>Who paid, who owes,<br/>and how much.</>}
        copy="Every expense can be split — by share, by percentage, by 'just me, thanks'. Settle up at month's end with one button."
        color={Hearth.clayDeep}
        reverse
        visual={<FeatureSplit />}
      />

      <FeatureRow
        kicker="Built for households with kids"
        title={<>The first allowance,<br/>without the lecture.</>}
        copy="Give each kid their own pocket. Set a goal — a scooter, a book — and watch it fill. The grown-ups stay in their own books."
        color="#3F5C32"
        visual={<FeatureKidsCorner />}
      />

      <FeaturesGrid />
      <Pricing />
      <Testimonials />
      <FinalCta />
      <Footer />
    </div>
  );
}

window.MarketingArtboard = MarketingArtboard;
