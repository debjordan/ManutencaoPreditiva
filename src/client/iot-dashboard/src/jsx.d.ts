// Fallback declarations for JSX intrinsic elements and the automatic runtime.
// This file is intentionally a script-free global declaration (no imports) so
// the TypeScript compiler always picks it up.
declare namespace JSX {
  interface IntrinsicElements {
    [elemName: string]: any;
  }
}

declare module 'react/jsx-runtime' {
  export const jsx: any;
  export const jsxs: any;
  export const jsxDEV: any;
  export default jsx;
}
