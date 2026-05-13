// Hearth design tokens — warm, earthy palette for ExpensesManager redesign.
// Loaded as a globals-exporting babel script before the artboard files.

const Hearth = {
  // Surfaces
  paper:   '#FAF6EE',  // page background, warm cream
  card:    '#FFFCF6',  // raised surface
  cardAlt: '#F3ECDD',  // hover / pressed
  border:  '#E8DECB',  // hairline
  divider: '#EFE6D3',

  // Ink
  ink:     '#23170E',  // deep cocoa, headings
  ink2:    '#574A3D',  // body
  mute:    '#8E8170',  // labels
  faint:   '#B8AB99',  // disabled

  // Brand + signals
  clay:    '#C8623E',  // primary accent — terracotta
  clayDeep:'#A24B2A',
  claySoft:'#F2D6C4',
  claySofter:'#F9E8DD',
  sage:    '#6B8E5A',  // positive / income / under budget
  sageSoft:'#DCE7CF',
  berry:   '#B5443F',  // alert / over budget
  berrySoft:'#F2D2CB',
  mustard: '#D6A23F',  // highlight / streak
  mustardSoft:'#F4E1B5',
  sky:     '#5C8C9E',  // info accent
  skySoft: '#CFE0E6',
};

// Inline style helpers
const sx = {
  serifDisplay: { fontFamily: '"Instrument Serif", "Iowan Old Style", Georgia, serif', fontWeight: 400, letterSpacing: '-0.01em' },
  sans:         { fontFamily: '"Manrope", ui-sans-serif, system-ui, -apple-system, "Segoe UI", Roboto, sans-serif' },
  mono:         { fontFamily: '"JetBrains Mono", ui-monospace, "SF Mono", Menlo, monospace', fontFeatureSettings: '"tnum"' },
};

// Logo mark — a small "h" hearth glyph made from soft geometry
function HearthMark({ size = 28, bg = Hearth.clay, fg = '#fff' }) {
  return (
    <span style={{
      display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
      width: size, height: size, borderRadius: size * 0.32,
      background: bg, color: fg, flexShrink: 0,
    }}>
      <svg width={size * 0.58} height={size * 0.58} viewBox="0 0 24 24" fill="none">
        <path d="M5 20V8c0-1 .8-2 2-2h2c1.2 0 2 .8 2 2v4h2V8c0-1 .8-2 2-2h2c1.2 0 2 .8 2 2v12" stroke={fg} strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" />
      </svg>
    </span>
  );
}

function Wordmark({ size = 18, accent = Hearth.clay, ink = Hearth.ink }) {
  return (
    <span style={{ ...sx.sans, fontWeight: 700, fontSize: size, letterSpacing: '-0.02em', color: ink, whiteSpace: 'nowrap' }}>
      Hearth<span style={{ color: accent }}>.</span>
    </span>
  );
}

// Money formatter
function money(n, currency = '€') {
  const sign = n < 0 ? '-' : '';
  const v = Math.abs(n);
  const s = v.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  return `${sign}${currency}${s}`;
}

// Simple SVG icons used across screens
const Icon = {
  plus: (p) => <svg width={p.size||16} height={p.size||16} viewBox="0 0 24 24" fill="none" {...p}><path d="M12 5v14M5 12h14" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/></svg>,
  arrowRight: (p) => <svg width={p.size||16} height={p.size||16} viewBox="0 0 24 24" fill="none" {...p}><path d="M5 12h14M13 5l7 7-7 7" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/></svg>,
  arrowUp: (p) => <svg width={p.size||14} height={p.size||14} viewBox="0 0 24 24" fill="none" {...p}><path d="M7 17L17 7M9 7h8v8" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round"/></svg>,
  arrowDown: (p) => <svg width={p.size||14} height={p.size||14} viewBox="0 0 24 24" fill="none" {...p}><path d="M17 7L7 17M15 17H7V9" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round"/></svg>,
  receipt: (p) => <svg width={p.size||18} height={p.size||18} viewBox="0 0 24 24" fill="none" {...p}><path d="M6 3h12v18l-3-2-3 2-3-2-3 2V3z" stroke="currentColor" strokeWidth="1.6" strokeLinejoin="round"/><path d="M9 8h6M9 12h6M9 16h4" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round"/></svg>,
  family: (p) => <svg width={p.size||18} height={p.size||18} viewBox="0 0 24 24" fill="none" {...p}><circle cx="9" cy="8" r="3" stroke="currentColor" strokeWidth="1.6"/><circle cx="17" cy="9" r="2.4" stroke="currentColor" strokeWidth="1.6"/><path d="M3 20c0-3 2.5-5.5 6-5.5s6 2.5 6 5.5M14.5 20c.5-2 2-3.5 4-3.5s3.5 1.5 3.5 3.5" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round"/></svg>,
  spark: (p) => <svg width={p.size||16} height={p.size||16} viewBox="0 0 24 24" fill="none" {...p}><path d="M12 3l1.8 5.4L19 10l-5.2 1.6L12 17l-1.8-5.4L5 10l5.2-1.6L12 3z" fill="currentColor"/></svg>,
  bell: (p) => <svg width={p.size||16} height={p.size||16} viewBox="0 0 24 24" fill="none" {...p}><path d="M6 8a6 6 0 1112 0c0 7 3 8 3 8H3s3-1 3-8M10 21a2 2 0 004 0" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round"/></svg>,
  search: (p) => <svg width={p.size||16} height={p.size||16} viewBox="0 0 24 24" fill="none" {...p}><circle cx="11" cy="11" r="7" stroke="currentColor" strokeWidth="1.8"/><path d="M20 20l-3.5-3.5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/></svg>,
  filter: (p) => <svg width={p.size||16} height={p.size||16} viewBox="0 0 24 24" fill="none" {...p}><path d="M4 5h16M7 12h10M10 19h4" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/></svg>,
  dot: (p) => <svg width={p.size||6} height={p.size||6} viewBox="0 0 6 6" {...p}><circle cx="3" cy="3" r="3" fill="currentColor"/></svg>,
  check: (p) => <svg width={p.size||14} height={p.size||14} viewBox="0 0 24 24" fill="none" {...p}><path d="M5 12l4.5 4.5L19 7" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" strokeLinejoin="round"/></svg>,
  more: (p) => <svg width={p.size||16} height={p.size||16} viewBox="0 0 24 24" fill="none" {...p}><circle cx="6" cy="12" r="1.6" fill="currentColor"/><circle cx="12" cy="12" r="1.6" fill="currentColor"/><circle cx="18" cy="12" r="1.6" fill="currentColor"/></svg>,
  globe: (p) => <svg width={p.size||16} height={p.size||16} viewBox="0 0 24 24" fill="none" {...p}><circle cx="12" cy="12" r="9" stroke="currentColor" strokeWidth="1.6"/><path d="M3 12h18M12 3c2.5 3 2.5 15 0 18M12 3c-2.5 3-2.5 15 0 18" stroke="currentColor" strokeWidth="1.6"/></svg>,
};

// Category palette (used by charts + tags)
const Categories = [
  { id: 'groceries',  name: 'Groceries',      glyph: '🛒', color: '#C8623E', soft: '#F2D6C4' },
  { id: 'kids',       name: 'Kids & school',  glyph: '🎒', color: '#D6A23F', soft: '#F4E1B5' },
  { id: 'home',       name: 'Home',           glyph: '🏠', color: '#6B8E5A', soft: '#DCE7CF' },
  { id: 'transport',  name: 'Transport',      glyph: '🚗', color: '#5C8C9E', soft: '#CFE0E6' },
  { id: 'dining',     name: 'Dining out',     glyph: '🍝', color: '#B5443F', soft: '#F2D2CB' },
  { id: 'health',     name: 'Health',         glyph: '🩺', color: '#8E6BA8', soft: '#E3D6EE' },
  { id: 'fun',        name: 'Fun & family',   glyph: '🎈', color: '#D97D60', soft: '#F4D7C9' },
  { id: 'bills',      name: 'Bills',          glyph: '⚡', color: '#574A3D', soft: '#E0D6C4' },
];

// Avatar — solid color circle with initials (kid-friendly)
function Avatar({ name = '', color = Hearth.clay, size = 28, ring = false }) {
  const initials = name.split(' ').filter(Boolean).slice(0,2).map(s => s[0]).join('').toUpperCase() || '?';
  return (
    <span style={{
      display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
      width: size, height: size, borderRadius: '50%',
      background: color, color: '#fff',
      fontFamily: sx.sans.fontFamily, fontWeight: 700, fontSize: size * 0.4,
      flexShrink: 0,
      boxShadow: ring ? `0 0 0 2px ${Hearth.card}, 0 0 0 ${size > 24 ? 4 : 3}px ${color}33` : 'none',
    }}>{initials}</span>
  );
}

// Pill / Tag
function Pill({ children, bg = Hearth.claySofter, color = Hearth.clayDeep, size = 'sm', style = {} }) {
  const pad = size === 'sm' ? '3px 9px' : '5px 12px';
  const fs = size === 'sm' ? 11 : 13;
  return (
    <span style={{
      display: 'inline-flex', alignItems: 'center', gap: 5,
      padding: pad, borderRadius: 999, background: bg, color,
      ...sx.sans, fontSize: fs, fontWeight: 600, letterSpacing: '0.01em',
      ...style,
    }}>{children}</span>
  );
}

Object.assign(window, { Hearth, sx, HearthMark, Wordmark, money, Icon, Categories, Avatar, Pill });
