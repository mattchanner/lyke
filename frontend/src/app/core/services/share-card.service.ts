import { Injectable } from '@angular/core';
import { KibbeFamily } from '../../models/style/kibbe.models';

interface FamilyTheme {
  background: string;
  accent: string;
  text: string;
}

const FAMILY_THEMES: Record<KibbeFamily, FamilyTheme> = {
  Dramatic: { background: '#0f0f1a', accent: '#7c6aff', text: '#ffffff' },
  Natural:  { background: '#2c1f10', accent: '#c8873a', text: '#ffffff' },
  Classic:  { background: '#0d2545', accent: '#6ea8d4', text: '#ffffff' },
  Gamine:   { background: '#1a0a0a', accent: '#e8454a', text: '#ffffff' },
  Romantic: { background: '#2a0f1e', accent: '#d97ba8', text: '#ffffff' },
};

@Injectable({ providedIn: 'root' })
export class ShareCardService {
  private readonly SIZE = 1080;

  async generate(family: KibbeFamily, tagline: string): Promise<Blob | null> {
    const canvas = document.createElement('canvas');
    canvas.width = this.SIZE;
    canvas.height = this.SIZE;
    const ctx = canvas.getContext('2d');
    if (!ctx) return null;

    const theme = FAMILY_THEMES[family];

    // Background
    ctx.fillStyle = theme.background;
    ctx.fillRect(0, 0, this.SIZE, this.SIZE);

    // Accent gradient overlay (top-left radial)
    const grad = ctx.createRadialGradient(0, 0, 0, 0, 0, this.SIZE * 0.85);
    grad.addColorStop(0, `${theme.accent}40`);
    grad.addColorStop(1, 'transparent');
    ctx.fillStyle = grad;
    ctx.fillRect(0, 0, this.SIZE, this.SIZE);

    // Thin accent border
    ctx.strokeStyle = `${theme.accent}66`;
    ctx.lineWidth = 3;
    ctx.strokeRect(40, 40, this.SIZE - 80, this.SIZE - 80);

    // "MY STYLE TYPE" label
    ctx.fillStyle = `${theme.accent}cc`;
    ctx.font = `500 ${this.SIZE * 0.032}px -apple-system, sans-serif`;
    ctx.letterSpacing = `${this.SIZE * 0.012}px`;
    ctx.textAlign = 'center';
    ctx.fillText('MY STYLE TYPE', this.SIZE / 2, this.SIZE * 0.36);

    // Family name
    ctx.fillStyle = theme.text;
    ctx.font = `800 ${this.SIZE * 0.125}px -apple-system, sans-serif`;
    ctx.letterSpacing = `-${this.SIZE * 0.004}px`;
    ctx.textAlign = 'center';
    ctx.fillText(family.toUpperCase(), this.SIZE / 2, this.SIZE * 0.52);

    // Divider line
    ctx.strokeStyle = `${theme.accent}66`;
    ctx.lineWidth = 1;
    ctx.beginPath();
    ctx.moveTo(this.SIZE * 0.3, this.SIZE * 0.575);
    ctx.lineTo(this.SIZE * 0.7, this.SIZE * 0.575);
    ctx.stroke();

    // Tagline
    ctx.fillStyle = `${theme.text}bb`;
    ctx.font = `italic ${this.SIZE * 0.038}px -apple-system, sans-serif`;
    ctx.letterSpacing = '0px';
    ctx.textAlign = 'center';
    this.wrapText(ctx, tagline, this.SIZE / 2, this.SIZE * 0.64, this.SIZE * 0.65, this.SIZE * 0.05);

    // LYKE wordmark
    ctx.fillStyle = theme.text;
    ctx.font = `700 ${this.SIZE * 0.06}px -apple-system, sans-serif`;
    ctx.letterSpacing = `-${this.SIZE * 0.002}px`;
    ctx.textAlign = 'center';
    ctx.fillText('LYKE', this.SIZE / 2, this.SIZE * 0.84);

    // Website
    ctx.fillStyle = `${theme.text}66`;
    ctx.font = `400 ${this.SIZE * 0.025}px -apple-system, sans-serif`;
    ctx.letterSpacing = `${this.SIZE * 0.003}px`;
    ctx.textAlign = 'center';
    ctx.fillText('be-lyke.clothing/style-quiz', this.SIZE / 2, this.SIZE * 0.89);

    return new Promise((resolve) => {
      canvas.toBlob((blob) => resolve(blob), 'image/png');
    });
  }

  private wrapText(
    ctx: CanvasRenderingContext2D,
    text: string,
    x: number,
    y: number,
    maxWidth: number,
    lineHeight: number,
  ): void {
    const words = text.split(' ');
    let line = '';
    let currentY = y;

    for (const word of words) {
      const test = line ? `${line} ${word}` : word;
      if (ctx.measureText(test).width > maxWidth && line) {
        ctx.fillText(line, x, currentY);
        line = word;
        currentY += lineHeight;
      } else {
        line = test;
      }
    }
    if (line) ctx.fillText(line, x, currentY);
  }
}
