/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{html,js,tsx,ts}'],
  theme: {
    fontFamily: {
      sans: 'Segoe UI, Roboto, sans-serif',
    },
    fontWeight: {
      light: 300,
      regular: 400,
      semibold: 600,
      bold: 700,
    },
    letterSpacing: {
      tightest: '-0.031em',
      tight: '0.016em',
      wide: '0.025em',
      wider: '0.031em',
      widest: '0.075em',
    },
    fontSize: {
      'heading-regular-1': [
        '3.75rem',
        {
          lineHeight: '52px',
          letterSpacing: '-0.031em',
        },
      ],
      'heading-regular-2': [
        '3rem',
        {
          lineHeight: '62px',
          letterSpacing: '-0.031em',
        },
      ],
      'heading-regular-3': [
        '2.5rem',
        {
          lineHeight: '48px',
          letterSpacing: '0.016em',
        },
      ],
      'heading-bold-3': [
        '2.5rem',
        {
          fontWeight: 'bold',
          lineHeight: '48px',
          letterSpacing: '0.016em',
        },
      ],
      'heading-regular-4': [
        '2rem',
        {
          lineHeight: '40px',
          letterSpacing: '0.016em',
        },
      ],
      'heading-bold-4': [
        '2rem',
        {
          lineHeight: '40px',
          letterSpacing: '0.016em',
          fontWeight: 'bold',
        },
      ],
      logo: [
        '2rem',
        {
          letterSpacing: '0.076em',
          fontWeight: 'bold',
        },
      ],
      'heading-regular-5': [
        '1.5rem',
        {
          lineHeight: '32px',
        },
      ],
      'heading-bold-5': [
        '1.5rem',
        {
          lineHeight: '32px',
        },
      ],
      'heading-regular-6': [
        '1.25rem',
        {
          lineHeight: '24px',
          letterSpacing: '0.016em',
        },
      ],
      'heading-bold-6': [
        '1.25rem',
        {
          lineHeight: '24px',
          letterSpacing: '0.016em',
        },
      ],
      'body-regular-1': [
        '1rem',
        {
          lineHeight: '22px',
          letterSpacing: '0.025em',
        },
      ],
      'body-light-1': [
        '1rem',
        {
          lineHeight: '22px',
          letterSpacing: '0.025em',
        },
      ],
      'body-bold-1': [
        '1rem',
        {
          letterSpacing: '0.025em',
        },
      ],
      'body-semibold-1': [
        '1rem',
        {
          lineHeight: '22px',
          letterSpacing: '0.025em',
        },
      ],
      'body-regular-2': [
        '0.875rem',
        {
          lineHeight: '20px',
          letterSpacing: '0.016em',
        },
      ],
      'body-light-2': [
        '0.875rem',
        {
          lineHeight: '20px',
          letterSpacing: '0.016em',
        },
      ],
      'body-bold-2': [
        '0.875rem',
        {
          lineHeight: '20px',
          letterSpacing: '0.016em',
        },
      ],
      'body-semibold-2': [
        '0.875rem',
        {
          lineHeight: '20px',
          letterSpacing: '0.016em',
        },
      ],
      'button-regular-1': [
        '0.875rem',
        {
          lineHeight: '20px',
          letterSpacing: '0.031em',
        },
      ],
      'button-semibold-1': [
        '0.875rem',
        {
          lineHeight: '20px',
          letterSpacing: '0.031em',
        },
      ],
      'caption-bold-1': [
        '0.75rem',
        {
          lineHeight: '20px',
          letterSpacing: '0.031em',
        },
      ],
      'caption-regular-1': [
        '0.75rem',
        {
          lineHeight: '20px',
          letterSpacing: '0.031em',
        },
      ],
      'caption-light-1': [
        '0.75rem',
        {
          lineHeight: '20px',
          letterSpacing: '0.031em',
        },
      ],
      'overline-bold-1': [
        '0.75rem',
        {
          lineHeight: '20px',
          letterSpacing: '0.075em',
        },
      ],
      'overline-regular-1': [
        '0.75rem',
        {
          lineHeight: '20px',
          letterSpacing: '0.075em',
        },
      ],
      'tooltip-1': [
        '0.625rem',
        {
          lineHeight: '14px',
          letterSpacing: '0.031em',
        },
      ],
      base: [
        '1rem',
        {
          lineHeight: '22px',
        },
      ],
      sm: [
        '0.875rem',
        {
          lineHeight: '20px',
        },
      ],
      xs: [
        '0.75rem',
        {
          lineHeight: '20px',
        },
      ],
      xxs: [
        '0.625rem',
        {
          lineHeight: '14px',
        },
      ],
    },
    colors: {
      black: '#000000',
      white: '#ffffff',
      gray: {
        100: '#FBFBFB',
        200: '#EBEBEB',
        300: '#D6D6D6',
        400: '#B8B8B8',
        500: '#999999',
        600: '#666666',
        700: '#474747',
        800: '#292929',
      },
      purple: {
        100: '#FAF3FF',
        200: '#F6EBFF',
        400: '#E4C2FF',
        500: '#C170FF',
        600: '#A429FF',
        700: '#7F00E0',
        800: '#5C00A3',
      },
      red: {
        200: '#F8DCDA',
        400: '#F7B9B6',
        500: '#EF736C',
        600: '#E83C33',
        700: '#CA1F16',
        800: '#A51A12',
      },
      yellow: {
        200: '#FFF3C2',
        400: '#FFEB99',
        500: '#FFDE5C',
        600: '#FFD119',
        700: '#E0B400',
        800: '#B89300',
      },
      lemon: {
        200: '#FAFFD6',
        400: '#F5FFAD',
        500: '#F1FF85',
        600: '#E6FF27',
        700: '#D3EB1E',
        800: '#B4CC00',
      },
      green: {
        200: '#DEF7E9',
        400: '#ADEBC8',
        500: '#7CDEA7',
        600: '#31C471',
        700: '#29A35E',
        800: '#1D7242',
      },
      blue: {
        100: '#2794F9',
      },
    },
    boxShadow: {
      DEFAULT: '4px 8px 16px 4px #D6D6D6, 0px 8px 24px #EBEBEB',
      1: '1px 1px 2px 1px #D6D6D6, -2px 2px 8px #D6D6D6',
      2: '1px 1px 4px 2px #EBEBEB, -4px 4px 16px #B8B8B8',
      3: '4px 8px 16px 4px #D6D6D6, 0px 8px 24px #EBEBEB',
      4: '4px 20px 24px 8px #D6D6D6, 0px 16px 32px #EBEBEB',
    },
    extend: {},
  },
  plugins: [],
};
