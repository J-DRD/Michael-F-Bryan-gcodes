# Gcodes Wiki Documentation

This folder contains the complete documentation for the Gcodes project wiki. These files are ready to be uploaded to the GitHub wiki or used as standalone documentation.

## Wiki Pages

### Core Documentation
- **[Home.md](Home.md)** - Main wiki homepage with project overview and navigation
- **[Getting-Started.md](Getting-Started.md)** - Installation guide and basic usage examples
- **[API-Reference.md](API-Reference.md)** - Complete API documentation for all classes and methods

### Developer Resources  
- **[Architecture-Overview.md](Architecture-Overview.md)** - Deep dive into library design and architecture
- **[Examples-and-Tutorials.md](Examples-and-Tutorials.md)** - Comprehensive code examples and tutorials
- **[Contributing-Guide.md](Contributing-Guide.md)** - Guidelines for contributing to the project

### Reference Materials
- **[Gcode-Reference.md](Gcode-Reference.md)** - G-code syntax and commands supported by the library
- **[Troubleshooting.md](Troubleshooting.md)** - Common issues and solutions
- **[CICD-Documentation.md](CICD-Documentation.md)** - GitHub Actions workflows and CI/CD processes

## How to Use These Files

### Option 1: GitHub Wiki Setup
1. Go to your repository on GitHub
2. Click the "Wiki" tab
3. If no wiki exists, click "Create the first page"
4. For each file in this folder:
   - Create a new wiki page with the filename (without .md extension)
   - Copy and paste the content from the corresponding file
   - Save the page

### Option 2: Documentation Website
These files can be used with static site generators like:
- **GitBook**
- **MkDocs** 
- **Docusaurus**
- **Jekyll**
- **Hugo**

### Option 3: Standalone Documentation
The files can be used as-is for offline documentation or included in releases.

## Wiki Navigation Structure

The wiki is organized with the following hierarchy:

```
Home (landing page)
├── Getting Started (new users)
├── API Reference (developers)
├── Architecture Overview (contributors)
├── Examples and Tutorials (all users)
├── G-code Reference (users)
├── Contributing Guide (contributors)
├── Troubleshooting (support)
└── CI/CD Documentation (maintainers)
```

## Maintenance

### Keeping Documentation Updated
- Update API Reference when adding new classes or methods
- Add new examples to Examples and Tutorials for significant features
- Update Getting Started guide for any installation changes
- Maintain Troubleshooting guide with newly discovered issues
- Update Architecture Overview for design changes

### Content Guidelines
- Keep language clear and accessible
- Include code examples for all concepts
- Provide step-by-step instructions where appropriate
- Cross-reference related sections
- Update screenshots if UI changes

## Integration with Repository

These wiki files complement the main repository documentation:

- **README.md** - Quick overview and getting started
- **Wiki** - Comprehensive documentation and tutorials
- **API Comments** - Inline code documentation
- **GitHub Issues** - Support and bug tracking

The wiki serves as the primary documentation hub, with the README providing a quick introduction and directing users to the wiki for detailed information.

## Content Summary

| Page | Target Audience | Content Type | Maintenance Frequency |
|------|----------------|--------------|---------------------|
| Home | All users | Overview/Navigation | Low |
| Getting Started | New users | Tutorial | Medium |
| API Reference | Developers | Reference | High |
| Architecture Overview | Contributors | Technical deep-dive | Medium |
| Examples and Tutorials | All users | Educational | Medium |
| G-code Reference | Users | Reference | Low |
| Contributing Guide | Contributors | Process documentation | Low |
| Troubleshooting | All users | Problem-solving | High |
| CI/CD Documentation | Maintainers | Technical reference | Medium |

Total word count: ~40,000 words across all pages, providing comprehensive coverage of the Gcodes library.