import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

// angular material modules
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatToolbarModule } from '@angular/material/toolbar';

// local module components
import { ToolbarComponent } from './toolbar/toolbar.component';
import { FooterComponent } from './footer/footer.component';

@NgModule({
  declarations: [ToolbarComponent, FooterComponent],
  imports: [CommonModule, MatButtonModule, MatDividerModule, MatToolbarModule],
  exports: [ToolbarComponent, FooterComponent],
})
export class CoreModule {}
