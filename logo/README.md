# NovaLang Logo Assets

This folder contains the official NovaLang logo and branding assets.

## Current Assets

- **`NOVA LANG.png`** - Main NovaLang logo (15KB)
  - Used in: README.md header
  - Specifications: PNG format, professional design
  - Dimensions: Optimized for documentation display

## Usage

### In README
The logo is displayed at the top of README.md using:
```markdown
<div align="center">
  <img src="logo/NOVA LANG.png" alt="NovaLang Logo" width="200"/>
  <br/>
  <em>Making functional programming accessible with familiar syntax!</em>
</div>
```

### In Documentation
Reference the logo in other documentation with:
```markdown
![NovaLang Logo](logo/NOVA LANG.png)
```

## Guidelines

- **File Format**: PNG with transparency preferred
- **Size**: Optimized for web/documentation use
- **Style**: Clean, modern design representing functional programming
- **Colors**: Professional palette matching project theme

## Git Handling

Logo files are included in version control with exceptions added to `.gitignore`:
```
!logo/*.png
!logo/*.jpg
!logo/*.svg
```

This ensures branding assets are preserved across all project distributions.
