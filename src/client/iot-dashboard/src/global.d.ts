// Ensure JSX types and the automatic runtime module are recognized by TypeScript/IDE
import * as React from 'react';

// Ensure JSX.IntrinsicElements exists. Prefer React's built-in typings when available,
// but provide a permissive fallback to avoid editor/TS server errors in mixed environments.
declare global {
  namespace JSX {
    // Try to reuse React's IntrinsicElements if present
    interface IntrinsicElements extends React.JSX.IntrinsicElements {}

    // Permissive fallback: allow any element name with arbitrary props
    // This prevents errors like "Property 'div' does not exist on type 'JSX.IntrinsicElements'"
    // while preserving existing React typings when available.
    // eslint-disable-next-line @typescript-eslint/no-empty-interface
    interface IntrinsicElements {
      [elemName: string]: any;
    }
  }
}

// Provide minimal declarations for the automatic JSX runtime module used by "react-jsx".
declare module 'react/jsx-runtime' {
  export const jsx: any;
  export const jsxs: any;
  export const jsxDEV: any;
  export default jsx;
}

export { };

