// Auth pages — Sign in & Register, in the Hearth direction.

function AuthShell({ side, children, formTitle, formSubtitle }) {
  // Split-screen layout: brand panel on one side, form on the other.
  return (
    <div style={{ background: Hearth.paper, height: '100%', display: 'flex' }}>
      {side === 'left' && <AuthBrandPanel />}
      <div style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', padding: 40 }}>
        <div style={{ width: '100%', maxWidth: 400 }}>
          <div style={{ marginBottom: 28 }}>
            <div style={{ ...sx.serifDisplay, fontSize: 38, color: Hearth.ink, lineHeight: 1.05, marginBottom: 8, letterSpacing: '-0.01em' }}>{formTitle}</div>
            <div style={{ ...sx.sans, fontSize: 15, color: Hearth.ink2, lineHeight: 1.5 }}>{formSubtitle}</div>
          </div>
          {children}
        </div>
      </div>
      {side === 'right' && <AuthBrandPanel />}
    </div>
  );
}

function AuthBrandPanel() {
  return (
    <div style={{
      flex: '0 0 46%',
      background: `linear-gradient(155deg, ${Hearth.clay} 0%, ${Hearth.clayDeep} 60%, #6E2F18 100%)`,
      color: Hearth.paper,
      padding: 56, display: 'flex', flexDirection: 'column', justifyContent: 'space-between',
      position: 'relative', overflow: 'hidden',
    }}>
      {/* Decorative soft circle */}
      <div style={{
        position: 'absolute', right: -80, top: -100, width: 380, height: 380,
        borderRadius: '50%', background: 'radial-gradient(circle, rgba(255,220,180,0.18) 0%, rgba(255,220,180,0) 70%)',
      }} />
      <div style={{ position: 'absolute', left: -60, bottom: -80, width: 280, height: 280, borderRadius: '50%', background: 'radial-gradient(circle, rgba(214,162,63,0.25) 0%, rgba(214,162,63,0) 70%)' }} />

      <div style={{ display: 'flex', alignItems: 'center', gap: 12, position: 'relative' }}>
        <HearthMark size={32} bg={Hearth.paper} fg={Hearth.clayDeep} />
        <Wordmark size={20} ink={Hearth.paper} accent={Hearth.mustard} />
      </div>

      <div style={{ position: 'relative' }}>
        <div style={{ ...sx.serifDisplay, fontSize: 44, lineHeight: 1.05, color: Hearth.paper, marginBottom: 18, letterSpacing: '-0.01em' }}>
          Welcome to the<br/>
          <em style={{ ...sx.serifDisplay, fontStyle: 'italic', color: Hearth.mustardSoft }}>kitchen counter.</em>
        </div>
        <div style={{ ...sx.sans, fontSize: 15, color: '#F4E1D3', maxWidth: 380, lineHeight: 1.55 }}>
          Where the family money lives — soft-edged, shared, and quietly intelligent.
        </div>

        {/* mini-receipt floats in */}
        <div style={{
          marginTop: 36, padding: 18, background: 'rgba(255,252,246,0.08)',
          backdropFilter: 'blur(10px)', border: '1px solid rgba(255,252,246,0.2)', borderRadius: 16,
          display: 'flex', alignItems: 'center', gap: 14,
        }}>
          <span style={{ fontSize: 26 }}>🛒</span>
          <div style={{ flex: 1 }}>
            <div style={{ ...sx.sans, fontSize: 14, fontWeight: 600, color: Hearth.paper }}>Weekly shop</div>
            <div style={{ ...sx.sans, fontSize: 12, color: '#F4D7C9' }}>Maya · split with Jonah</div>
          </div>
          <div style={{ ...sx.serifDisplay, fontSize: 24, color: Hearth.paper }}>€80.90</div>
        </div>
      </div>

      <div style={{ position: 'relative', ...sx.sans, fontSize: 12, color: '#F4D7C9', display: 'flex', justifyContent: 'space-between' }}>
        <span>© Hearth, 2026</span>
        <span style={{ display: 'inline-flex', alignItems: 'center', gap: 6 }}><Icon.globe size={12} /> English</span>
      </div>
    </div>
  );
}

function Field({ label, hint, children, error }) {
  return (
    <div style={{ marginBottom: 16 }}>
      <label style={{ ...sx.sans, display: 'block', fontSize: 13, fontWeight: 600, color: Hearth.ink, marginBottom: 6 }}>{label}</label>
      {children}
      {hint && <div style={{ ...sx.sans, fontSize: 12, color: Hearth.mute, marginTop: 6 }}>{hint}</div>}
      {error && <div style={{ ...sx.sans, fontSize: 12, color: Hearth.berry, marginTop: 6 }}>{error}</div>}
    </div>
  );
}

const inputStyle = {
  ...sx.sans, width: '100%', boxSizing: 'border-box',
  padding: '13px 14px', fontSize: 15, color: Hearth.ink,
  background: Hearth.card, border: `1px solid ${Hearth.border}`, borderRadius: 12,
  outline: 'none', transition: 'border-color .15s, box-shadow .15s',
};

const primaryBtn = {
  ...sx.sans, width: '100%', padding: '13px 18px', fontSize: 15, fontWeight: 600,
  background: Hearth.ink, color: Hearth.paper, border: 'none', borderRadius: 12, cursor: 'pointer',
  display: 'inline-flex', alignItems: 'center', justifyContent: 'center', gap: 8,
  boxShadow: '0 8px 20px -10px rgba(30,20,10,.5)',
};

function SignInArtboard() {
  return (
    <AuthShell
      side="left"
      formTitle={<>Welcome back.</>}
      formSubtitle={<>Sign in to see where September went.</>}
    >
      <Field label="Email">
        <input style={inputStyle} defaultValue="maya@familyrivera.com" />
      </Field>
      <Field label="Password" hint={<span style={{ ...sx.sans }}><a style={{ color: Hearth.clayDeep, fontWeight: 600, textDecoration: 'none' }}>Forgot your password?</a></span>}>
        <div style={{ position: 'relative' }}>
          <input type="password" style={inputStyle} defaultValue="••••••••••" />
          <span style={{ position: 'absolute', right: 14, top: '50%', transform: 'translateY(-50%)', color: Hearth.mute, ...sx.sans, fontSize: 13, cursor: 'pointer' }}>Show</span>
        </div>
      </Field>
      <div style={{ marginTop: 24 }}>
        <button style={primaryBtn}>Sign in <Icon.arrowRight size={16} /></button>
      </div>
      <div style={{ ...sx.sans, fontSize: 14, color: Hearth.ink2, textAlign: 'center', marginTop: 22 }}>
        First time here? <a style={{ color: Hearth.clayDeep, fontWeight: 600, textDecoration: 'none' }}>Make a household account →</a>
      </div>
    </AuthShell>
  );
}

function RegisterArtboard() {
  return (
    <AuthShell
      side="right"
      formTitle={<>Start your household.</>}
      formSubtitle={<>Two minutes. Then invite the rest of the family.</>}
    >
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
        <Field label="First name"><input style={inputStyle} defaultValue="Maya" /></Field>
        <Field label="Last name"><input style={inputStyle} defaultValue="Rivera" /></Field>
      </div>
      <Field label="Email"><input style={inputStyle} defaultValue="maya@familyrivera.com" /></Field>
      <Field label="Password" hint="At least 8 characters — we'll grade it as you type.">
        <input type="password" style={inputStyle} defaultValue="••••••••••" />
        <div style={{ display: 'flex', gap: 4, marginTop: 8 }}>
          {[Hearth.clay, Hearth.clay, Hearth.mustard, Hearth.cardAlt].map((c,i) => (
            <div key={i} style={{ flex: 1, height: 4, borderRadius: 4, background: c }} />
          ))}
        </div>
        <div style={{ ...sx.sans, fontSize: 11, color: Hearth.mute, marginTop: 6 }}>Strength: medium</div>
      </Field>
      <div style={{ marginTop: 8 }}>
        <button style={primaryBtn}>Create household <Icon.arrowRight size={16} /></button>
      </div>
      <div style={{ ...sx.sans, fontSize: 12, color: Hearth.mute, textAlign: 'center', marginTop: 14, lineHeight: 1.5 }}>
        By continuing, you agree to our <a style={{ color: Hearth.ink2, textDecoration: 'underline' }}>Terms</a> and <a style={{ color: Hearth.ink2, textDecoration: 'underline' }}>Privacy</a>. We will never sell what your family buys.
      </div>
    </AuthShell>
  );
}

window.SignInArtboard = SignInArtboard;
window.RegisterArtboard = RegisterArtboard;
