import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { NgxsModule } from '@ngxs/store';

import { AdminContainerComponent } from './admin-container/admin-container.component';
import { WordsState } from './words.state';

const routes: Routes = [{ path: '', component: AdminContainerComponent }];

@NgModule({
  declarations: [AdminContainerComponent],
  imports: [
    CommonModule, 
    RouterModule.forChild(routes),
    NgxsModule.forFeature([WordsState])
  ],
})
export class AdminModule {}
