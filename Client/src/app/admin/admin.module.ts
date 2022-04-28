import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { RouterModule, Routes } from '@angular/router';
import { AdminContainerComponent } from './admin-container/admin-container.component';

const routes: Routes = [{ path: '', component: AdminContainerComponent }];

@NgModule({
  declarations: [AdminContainerComponent],
  imports: [CommonModule, RouterModule.forChild(routes)],
})
export class AdminModule {}
